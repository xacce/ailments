using NUnit.Compatibility;
using Src.PackageCandidate.Attributer;
using Sufferenger;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GameReady.Ailments.Runtime.Jobs
{
    [BurstCompile]
    [WithAll(typeof(Simulate))]
    internal partial struct AilmentResolverJob : IJobEntity, IJobEntityChunkBeginEnd
    {
        [ReadOnly] public NativeHashMap<int2, int> dotDefensiveAttributes;

        // public EntityCommandBuffer.ParallelWriter ecb;
        // [ReadOnly] public NativeHashMap<int, AilmentDatabaseElement> db;
        [NativeDisableContainerSafetyRestriction]
        private NativeHashMap<int, float> _storedAttributes;

        [BurstCompile]
        private void Execute(
            ref AilmentCarrier carrier,
            DynamicBuffer<ConstructedAilment> active,
            DynamicBuffer<ActiveAilmentCounter> counter,
            DynamicBuffer<ApplyAilment> applies,
            DynamicBuffer<AttributeValue> attributeValues,
            DynamicBuffer<DealDamage> dmgBuffer, //TODO remove me and call it inside queue
            AttributeDependency attributeDependency,
            Entity entity)
        {
            if (active.IsEmpty && applies.IsEmpty) return;
            _storedAttributes.Clear();
            var ctx = new AilmentConstructedContext() { carrier = carrier, baseValues = _storedAttributes };
            for (int i = 0; i < applies.Length; i++)
            {
                var apply = applies[i];
                var result = Helper.TryAddAilment(apply.constructed, ref active, ref counter, out int ailmentIndex);
                if (result)
                {
                    // ref var blob = ref apply.constructed.BlobAssetReference_0.Value;
                    active[ailmentIndex].ailment.OnFresh(ref ctx);
                }
            }

            applies.Clear();

            for (int i = active.Length - 1; i >= 0; i--)
            {
                var activeAilment = active[i];
                // ref var blob = ref ailment.blob.Value;
                bool expired = false;
                activeAilment.root.duration--;
                activeAilment.ailment.OnTick(ref ctx);
                active[i] = activeAilment;

                if (activeAilment.root.duration <= 0)
                {
                    active.RemoveAt(i);
                    expired = true;
                    Helper.UpdateAilmentCount(ref counter, activeAilment.root.stackGroupId, 1);
                }

                if (expired)
                {
                    activeAilment.ailment.OnExpired(ref ctx);
                }
            }

            if (ctx.attributesStored)
            {
                var attributesRw = new AttributesRw() { values = attributeValues.AsNativeArray(), originDependencies = attributeDependency };
            }

            if (ctx.dmgStored)
            {
                var outDmg = new int3x3();
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (dotDefensiveAttributes.TryGetValue(new int2(i, j), out var defensive))
                        {
                            var dmg = AttributesHelper.ApplyAddictive(ctx.accomulatedDmg[i][j], attributeValues.GetCurrent(defensive));
                            if (dmg > 0)
                            {
                                // Debug.Log($"{ctx.accomulatedDmg[i][j]} -> {dmg}");
                                outDmg[i][j] = (int)dmg;
                            }
                        }
                    }
                }

                dmgBuffer.Add(new DealDamage(outDmg));
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