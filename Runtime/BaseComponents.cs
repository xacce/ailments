using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Core.Hybrid;
using GameReady.Ailments.Hybrid;
using Src.PackageCandidate.Attributer;
using Src.PackageCandidate.Attributer.Authoring;
using Src.PackageCandidate.GameReady.Ailments.Hybrid;
using Trove.PolymorphicStructs;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GameReady.Ailments.Runtime
{
    public partial struct AilmentCarrier : IComponentData
    {
        public AilmentTag tags;
    }


    [PolymorphicStructInterface]
    public interface IAilment
    {
        public void OnFresh(ref AilmentConstructedContext ctx);
        public void OnTick(ref AilmentConstructedContext ctx);
        public void OnExpired(ref AilmentConstructedContext ctx);
    }

    [PolymorphicStructInterface]
    public interface IAilmentConstructor
    {
        public bool TryConstruct(AilmentPreConstructedContext ctx, out ConstructedAilment constructed);
    }


    public partial struct AilmentConstructor
    {
    }


    public partial struct AilmentConstructedContext
    {
        public AilmentCarrier carrier;
        public int3x3 accomulatedDmg;
        public NativeHashMap<int, float> baseValues;
        public bool dmgStored;
        public bool attributesStored;

        public void AddBaseAttributeValue(int attributeIndex, float value)
        {
            if (!baseValues.ContainsKey(attributeIndex)) baseValues[attributeIndex] = value;
            baseValues[attributeIndex] += value;
            attributesStored = true;
        }

        public void StoreDamage(int3x3 dmg)
        {
            accomulatedDmg += dmg;
            dmgStored = true;
        }
    }

    public partial struct AilmentPreConstructedContext
    {
        [ReadOnly] public DynamicBuffer<AttributeValue> attributes;
#if DAMAGE_MODULE
        public int3x3 inputDmg;
        public int2 inputDmgIndex;
#endif
    }

    public struct AilmentRootData
    {
        public int stackGroupId;
        public uint duration;
        public uint maxStacks;
        public uint split;
        public StackMode stackMode;
    }

    [Serializable]
    public partial struct AilmentRootConstructorBaked
    {
        public AttributeSo scaleDurationAttributeIndex;
        public AilmentRootConstructor.ScaleMode durationScaleMode;
        public AttributeSo scaleMaxStacksAttributeIndex;
        public AilmentRootConstructor.ScaleMode maxStacksScaleMode;
        public AttributeSo applyStacksAttributeIndex;
        public AilmentRootConstructor.ScaleMode applySacksScaleMode;
        public uint durationTicks;
        public uint maxStacks;
        public uint applyStacks;
        public StackMode stackMode;

        [PickId(typeof(AilmentBakedSo))]
        public int stackGroupId;

        public AilmentRootConstructor Bake()
        {
            return new AilmentRootConstructor
            {
                stackGroupId = stackGroupId,
                applyStacksAttributeIndex = applyStacksAttributeIndex ? applyStacksAttributeIndex.id : 0,
                scaleDurationAttributeIndex = scaleDurationAttributeIndex ? scaleDurationAttributeIndex.id : 0,
                scaleMaxStacksAttributeIndex = scaleMaxStacksAttributeIndex ? scaleMaxStacksAttributeIndex.id : 0,
                applyStacks = applyStacks,
                durationTicks = durationTicks,
                maxStacks = maxStacks,
                stackMode = stackMode,
                durationScaleMode = durationScaleMode,
                applySacksScaleMode = applySacksScaleMode,
                maxStacksScaleMode = maxStacksScaleMode,
            };
        }
    }

    public partial struct AilmentRootConstructor
    {
        public enum ScaleMode
        {
            Nothing,
            Multiplier,
            Flat,
            Override,
        }


        public int scaleDurationAttributeIndex;
        public ScaleMode durationScaleMode;


        public int scaleMaxStacksAttributeIndex;
        public ScaleMode maxStacksScaleMode;


        public int applyStacksAttributeIndex;
        public ScaleMode applySacksScaleMode;
        public uint durationTicks;
        public uint maxStacks;
        public uint applyStacks;
        public StackMode stackMode;
        public int stackGroupId;

        public static AilmentRootConstructor Default = new AilmentRootConstructor()
        {
            applyStacks = 1,
            maxStacks = 1,
            durationTicks = 120,
            stackMode = StackMode.Override,
        };

        public static uint GetScaled(ScaleMode mode, uint origin, uint mod)
        {
            switch (mode)
            {
                case ScaleMode.Flat:
                    return math.max((uint)(origin + mod), 0);
                case ScaleMode.Multiplier:
                    return (uint)AttributesHelper.ApplyAddictive((float)origin, (float)mod);
                case ScaleMode.Override:
                    return math.max((uint)(mod), 0);
                case ScaleMode.Nothing:
                    return (uint)origin;
            }

            return 0;
        }

        public static float GetScaled(ScaleMode mode, float origin, float mod)
        {
            switch (mode)
            {
                case ScaleMode.Flat:
                    return math.max((uint)(origin + mod), 0);
                case ScaleMode.Multiplier:
                    return (float)AttributesHelper.ApplyAddictive((float)origin, (float)mod);
                case ScaleMode.Override:
                    return math.max((uint)(mod), 0);
                case ScaleMode.Nothing:
                    return (uint)origin;
            }

            return 0;
        }

        public AilmentRootData Construct(AilmentPreConstructedContext ctx)
        {
            return new AilmentRootData()
            {
                duration = GetScaled(durationScaleMode, durationTicks, (uint)ctx.attributes.GetCurrent(scaleDurationAttributeIndex)),
                maxStacks = GetScaled(maxStacksScaleMode, maxStacks, (uint)ctx.attributes.GetCurrent(scaleMaxStacksAttributeIndex)),
                stackGroupId = stackGroupId,
                stackMode = stackMode,
                split = GetScaled(applySacksScaleMode, applyStacks, (uint)ctx.attributes.GetCurrent(applyStacksAttributeIndex)),
            };
        }
    }

    [InternalBufferCapacity(0)]
    public partial struct ConstructedAilment : IBufferElementData
    {
        public AilmentRootData root;

        public Ailment ailment;
        // public BlobAssetReference<AilmentRootConstructor> constructor;
    }


    public struct AilmentConstructors
    {
        public bool empty;
        public BlobArray<AilmentConstructor> construct;

        public NativeArray<ConstructedAilment> Construct(AilmentPreConstructedContext ctx)
        {
            var ailments = new NativeList<ConstructedAilment>(construct.Length, Allocator.Temp); //todo shared array
            for (int i = 0; i < construct.Length; i++)
            {
                ref var c = ref construct[i];
                if (c.TryConstruct(ctx, out var a)) ailments.Add(a);
            }

            return ailments;
        }

        public bool TryConstruct(AilmentPreConstructedContext ctx, int index, out ConstructedAilment constructed)
        {
            return construct[index].TryConstruct(ctx, out constructed);
        }

#if UNITY_EDITOR
        public static BlobAssetReference<AilmentConstructors> Create(AilmentBakedSo[] baked, IBaker baker)
        {
            BlobBuilder builder = new BlobBuilder(Allocator.Temp);
            ref var definition = ref builder.ConstructRoot<AilmentConstructors>();
            if (baked == null || baked.Length == 0)
            {
                definition.empty = true;
            }
            else
            {
                var csts = builder.Allocate(ref definition.construct, baked.Length);
                for (int i = 0; i < baked.Length; i++)
                {
                    Debug.Log($"Ailment was baked, {baked[i].GetType()}, {baked[i]}");
                    baked[i].BakeAilment(ref builder, ref csts[i]);
                }
            }

            var blobReference = builder.CreateBlobAssetReference<AilmentConstructors>(Allocator.Persistent);
            baker.AddBlobAsset(ref blobReference, out _);
            builder.Dispose();


            return blobReference;
        }

        public static void Create(ref AilmentConstructors definition, ref BlobBuilder builder, AilmentBakedSo[] baked)
        {
            if (baked == null || baked.Length == 0)
            {
                definition.empty = true;
            }
            else
            {
                var csts = builder.Allocate(ref definition.construct, baked.Length);
                for (int i = 0; i < baked.Length; i++)
                {
                    baked[i].BakeAilment(ref builder, ref csts[i]);
                }
            }
        }
#endif
    }

    [InternalBufferCapacity(0)]
    public partial struct ActiveAilmentCounter : IBufferElementData
    {
        public int id;
        public int counter;
    }

    [InternalBufferCapacity(0)]
    public partial struct ApplyAilment : IBufferElementData
    {
        public ConstructedAilment constructed;
    }
}