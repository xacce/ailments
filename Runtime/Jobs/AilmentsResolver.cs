using Src.OneShoot;
using Src.PackageCandidate.Ailments.Runtime;
using Src.PackageCandidate.Attributer;
using Sufferenger;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace GameReady.Ailments.Runtime.Jobs
{
    [BurstCompile]
    [WithAll(typeof(Simulate))]
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
            DynamicBuffer<AilmentRuntime> active,
            DynamicBuffer<ActiveAilmentCounter> counter,
            DynamicBuffer<ApplyAilment> applies,
            DynamicBuffer<DealDamage> dmgBuffer, //TODO remove me and call it inside queue
            Entity entity)
        {
            if (active.IsEmpty && applies.IsEmpty) return;
            _storedAttributes.Clear();
            var ctx = new AilmentCreatedContext() { carrier = carrier, baseValues = _storedAttributes, deltaTime = deltaTime };
            var attributesRw = new AttributerRw<AttributerRwContext>(attributerRwCtx, entity);
            for (int i = 0; i < applies.Length; i++)
            {
                var apply = applies[i];
                AilmentBlob.AffectDefensive(ref apply.ailmentRuntime, attributesRw.values);
                var result = Helper.TryAddAilment(ref ctx, apply.ailmentRuntime, ref active, ref counter, out int ailmentIndex);
                if (result)
                {
                    evts.Set(GameOneShootShortEventType.AilmentsUpdated);
                }
                // if (result)
                // {
                //     // ref var blob = ref apply.constructed.BlobAssetReference_0.Value;
                //     active[ailmentIndex].ailment.OnFresh(ref ctx,active[ailmentIndex]);
                // }
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
                    Helper.UpdateAilmentCount(ref counter, activeAilment.rootRuntimeData.stackGroupId, 1);
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