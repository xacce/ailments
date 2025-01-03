﻿using System.Runtime.CompilerServices;
using Core.Hybrid;
using Core.Runtime;
using Src.PackageCandidate.Ailments.Runtime;
using Src.PackageCandidate.Attributer;
using Src.PackageCandidate.Attributer.Authoring;
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


    public partial struct AilmentCreatedContext
    {
        public AilmentCarrier carrier;
        public int3x3 accomulatedDmg;
        public NativeHashMap<int, float> baseValues;
        public bool dmgStored;
        public bool attributesStored;

        public void AddBaseAttributeValue(int attributeIndex, float value)
        {
            if (!baseValues.ContainsKey(attributeIndex)) baseValues[attributeIndex] = value;
            else baseValues[attributeIndex] += value;
            attributesStored = true;
        }

        public void StoreDamage(int3x3 dmg)
        {
            accomulatedDmg += dmg;
            dmgStored = true;
        }
    }

    public partial struct AilmentCreationContext
    {
        public Unity.Mathematics.Random rnd;
        [ReadOnly] public DynamicBuffer<AttributeValue> attributes;
#if DAMAGE_MODULE
        public int3x3 inputDmg;
        public int2 inputDmgIndex;
#endif
    }

    public partial struct AilmentRootRuntimeData
    {
        public int stackGroupId;
        public int duration;
        public int maxStacks;
        public int split;
        public StackMode stackMode;
    }

    public partial struct AilmentBlob
    {
        public enum ScaleMode
        {
            Nothing,
            Multiplier,
            Flat,
            Override,
        }


        public struct Root
        {
            public int scaleDurationAttributeIndex;
            public ScaleMode durationScaleMode;
            public int scaleMaxStacksAttributeIndex;
            public ScaleMode maxStacksScaleMode;
            public int applyStacksAttributeIndex;
            public ScaleMode applyStacksScaleMode;
            public int applyValidationRandomAttribute;

            public int defensiveScaleDurationAttributeIndex;
            public ScaleMode defensiveScaleDurationMode;
            public int defensiveScaleMaxStacksAttributeIndex;
            public ScaleMode defensiveScaleMaxStacksMode;


            public int durationTicks;
            public int maxStacks;
            public int applyStacks;
            public StackMode stackMode;
            public int stackGroupId;
            public LocalizationStringTableReferenceBaked title;
            public LocalizationStringTableReferenceBaked description;
        }

        public struct PolyData
        {
            public int3x3 intMatrix;
            public AttributeShortList attributes;
            public int i1;
            public int i2;
            public int f1;
            public int f2;
        }

        // public partial struct GameData
        // {
        //     public bool 
        // }

        public Root root;
        public PolyData polyData;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Validate(ref AilmentCreationContext ctx, BlobAssetReference<AilmentBlob> blobRef)
        {
            ref var blob = ref blobRef.Value.root;
            if (blob.applyValidationRandomAttribute >= 0)
            {
                return ctx.rnd.NextFloat() <= ctx.attributes.GetCurrent(blob.applyValidationRandomAttribute);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetScaled(ScaleMode mode, int origin, int mod)
        {
            switch (mode)
            {
                case ScaleMode.Flat:
                    return math.max((origin + mod), 0);
                case ScaleMode.Multiplier:
                    return (int)AttributesHelper.ApplyAddictive((float)origin, (float)mod);
                case ScaleMode.Override:
                    return math.max((mod), 0);
                case ScaleMode.Nothing:
                    return origin;
            }

            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetScaled(ScaleMode mode, float origin, float mod)
        {
            switch (mode)
            {
                case ScaleMode.Flat:
                    return math.max((origin + mod), 0);
                case ScaleMode.Multiplier:
                    return (float)AttributesHelper.ApplyAddictive((float)origin, (float)mod);
                case ScaleMode.Override:
                    return math.max((mod), 0);
                case ScaleMode.Nothing:
                    return origin;
            }

            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AffectDefensive(ref AilmentRuntime runtime, DynamicBuffer<AttributeValue> attrs)
        {
            ref var root = ref runtime.blob.Value.root;
            AffectDefensive(ref runtime.rootRuntimeData, ref root, attrs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AffectDefensive(ref AilmentRootRuntimeData data, ref Root root, DynamicBuffer<AttributeValue> attrs)
        {
            data.duration = GetScaled(root.defensiveScaleDurationMode, data.duration, (int)attrs.GetCurrent(root.defensiveScaleDurationAttributeIndex));
            data.maxStacks =
                GetScaled(root.defensiveScaleMaxStacksMode, data.maxStacks, (int)attrs.GetCurrent(root.defensiveScaleMaxStacksAttributeIndex));
        }

        public AilmentRootRuntimeData Create(AilmentCreationContext ctx)
        {
            return new AilmentRootRuntimeData()
            {
                duration = GetScaled(root.durationScaleMode, root.durationTicks, (int)ctx.attributes.GetCurrent(root.scaleDurationAttributeIndex)),
                maxStacks = GetScaled(root.maxStacksScaleMode, root.maxStacks, (int)ctx.attributes.GetCurrent(root.scaleMaxStacksAttributeIndex)),
                stackGroupId = root.stackGroupId,
                stackMode = root.stackMode,
                split = GetScaled(root.applyStacksScaleMode, root.applyStacks, (int)ctx.attributes.GetCurrent(root.applyStacksAttributeIndex)),
            };
        }
    }

    [InternalBufferCapacity(0)]
    public partial struct AilmentRuntime : IBufferElementData
    {
        public BlobAssetReference<AilmentBlob> blob;
        public AilmentRootRuntimeData rootRuntimeData;
        public Ailment ailment;
    }

    [InternalBufferCapacity(0)]
    public partial struct AilmentElement : IBufferElementData
    {
        public BlobAssetReference<AilmentBlob> blob;
        public Ailment ailment;
    }

    public static class AilmentElementExtender
    {
        public static void ConstructAll(this INativeList<AilmentElement> ailments, ref AilmentCreationContext ctx, DynamicBuffer<ApplyAilment> to)
        {
            for (int i = 0; i < ailments.Length; i++)
            {
                var ailment = ailments[i];
                if (ailment.ailment.Validate(ref ctx, ailment.blob))
                {
                    to.Add(new ApplyAilment() { ailmentRuntime = ailments[i].ailment.Create(ref ctx, ailments[i].blob) });
                }
            }
        }
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
        public AilmentRuntime ailmentRuntime;
    }
}