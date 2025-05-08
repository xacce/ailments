using Core.Runtime.LatiosHashMap.Latios;
using Src.OneShoot;
using Src.PackageCandidate.Ailments.Runtime;
using Src.PackageCandidate.Attributer;
using Src.PackageCandidate.LogTest;
using Sufferenger;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

namespace GameReady.Ailments.Runtime.Jobs
{
    [BurstCompile]
    [WithNone(typeof(IsDead))]
    internal partial struct AilmentResolverJob : IJobEntity, IJobEntityChunkBeginEnd
    {
#if DAMAGE_MODULE
        [ReadOnly] public NativeHashMap<int2, int> dotDefensiveAttributes;
#endif
        // public EntityCommandBuffer.ParallelWriter ecb;
        // [ReadOnly] public NativeHashMap<int, AilmentDatabaseElement> db;
        [NativeDisableContainerSafetyRestriction]
        private NativeHashMap<int, float> _storedAttributes;

        public AttributerRwContext attributerRwCtx;
        public float deltaTime;

        [BurstCompile]
        private void Execute(
            ref AilmentCarrier carrier,
            ref GameOneShootShortEvent evts,
            DynamicBuffer<ActiveAilmentArrayMap> _mapped,
            DynamicBuffer<AilmentRuntime> active,
            DynamicBuffer<ApplyAilment> applies,
            DynamicBuffer<DealDamage> dmgBuffer, //TODO remove me and call it inside queue
            Entity entity)
        {
            if (active.IsEmpty && applies.IsEmpty) return;
            GameDebug.Spam("Ailment", $"Has new incoming ailments", entity);
            _storedAttributes.Clear();
            GameDebug.Spam("Ailment", $"Create hash map", entity);
            var map = new DynamicHashMap<int, AilmentInfo>(_mapped.Reinterpret<DynamicHashMap<int, AilmentInfo>.Pair>());
            GameDebug.Spam("Ailment", $"Create context", entity);
            var ctx = new AilmentCreatedContext() { carrier = carrier, baseValues = _storedAttributes, deltaTime = deltaTime };
            GameDebug.Spam("Ailment", $"Create attributes", entity);
            var attributesRw = new AttributerRw<AttributerRwContext>(attributerRwCtx, entity);
            for (int i = 0; i < applies.Length; i++)
            {
                GameDebug.Spam("Ailment", $"Resolve: {i} incoming ailment", entity);
                var apply = applies[i];
                GameDebug.Spam("Ailment", $"Apply defensive layers for {i} incoming ailment", entity);
                AilmentBlob.AffectDefensive(ref apply.ailmentRuntime, attributesRw.values);

                GameDebug.Spam("Ailment", $"Trying to add to storage {i} incoming ailment", entity);
                var result = Helper.TryAddAilment(ref ctx, apply.ailmentRuntime, ref map, ref active, entity, out int ailmentIndex);
                GameDebug.Spam("Ailment", $"Add result : {result} for {i} incoming ailment", entity);
                if (result)
                {
                    evts.Set(GameOneShootShortEventType.AilmentsUpdated);
                }
            }

            applies.Clear();

            for (int i = active.Length - 1; i >= 0; i--)
            {
                var activeAilment = active[i];
                bool expired = false;
                activeAilment.rootRuntimeData.duration -= deltaTime;
                activeAilment.ailment.OnTick(ref ctx, activeAilment);
                active[i] = activeAilment;

                if (activeAilment.rootRuntimeData.duration <= 0)
                {
                    active.RemoveAt(i);
                    expired = true;
                    if (map.TryGetValue(activeAilment.rootRuntimeData.stackGroupId, out var stackGroupCount))
                    {
                        stackGroupCount.stacksCount--;
                        if (stackGroupCount.stacksCount <= 0)
                        {
                            map.Remove(activeAilment.rootRuntimeData.stackGroupId);
                        }
                        else
                        {
                            if (activeAilment.rootRuntimeData.duration > stackGroupCount.duration)
                            {
                                stackGroupCount.duration = activeAilment.rootRuntimeData.duration;
                            }
                            map.AddOrSet(activeAilment.rootRuntimeData.stackGroupId, stackGroupCount);
                        }

                        GameDebug.Spam("Ailment", $"Ailment {activeAilment.rootRuntimeData.stackGroupId} was expired");
                    }

                    evts.Set(GameOneShootShortEventType.AilmentsUpdated);
                }

                if (expired)
                {
                    activeAilment.ailment.OnExpired(ref ctx, activeAilment);
                }
            }

            if (ctx.attributesStored)
            {
                var en = ctx.baseValues.GetEnumerator();
                while (en.MoveNext())
                {
                    var kv = en.Current;
                    attributesRw.AddBase(kv.Key, kv.Value);
                }

                en.Dispose();
            }

            if (ctx.dmgStored)
            {
                // var outDmg = new float3x3();
                // for (int i = 0; i < 3; i++)
                // {
                //     for (int j = 0; j < 3; j++)
                //     {
                //         if (dotDefensiveAttributes.TryGetValue(new int2(i, j), out var defensive))
                //         {
                //             var dmg = AttributesHelper.ApplyAddictive(ctx.accomulatedDmg[i][j], attributeValues.GetCurrent(defensive));
                //             if (dmg > 0)
                //             {
                //                 // Debug.Log($"{ctx.accomulatedDmg[i][j]} -> {dmg}");
                //                 outDmg[i][j] = (int)dmg;
                //             }
                //         }
                //     }
                // }

                dmgBuffer.Add(DealDamage.CreateData(ctx.accomulatedDmg, CarryDamage.Tag.Armour | CarryDamage.Tag.Dot));

                // Debug.Log("Has stored dmg");
            }

            carrier = ctx.carrier;
        }

        public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            _storedAttributes = new NativeHashMap<int, float>(10, Allocator.Temp);
            return true;
        }

        public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted)
        {
        }
    }
}