using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Core.Runtime;
using GameReady.Ailments.Runtime;
using Src.PackageCandidate.Attributer;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Src.PackageCandidate.Ailments.Runtime
{
    public partial struct AilmentCarrier : IComponentData
    {
        public AilmentTag tags;
    }


    public partial struct AilmentCreatedContext
    {
        public float deltaTime;
        public AilmentCarrier carrier;
        public float3x3 accomulatedDmg;
        public NativeHashMap<int, float> baseValues;
        [MarshalAs(UnmanagedType.U1)]
        public bool dmgStored;
        [MarshalAs(UnmanagedType.U1)]
        public bool attributesStored;

        public void AddBaseAttributeValue(int attributeIndex, float value)
        {
            if (!baseValues.ContainsKey(attributeIndex)) baseValues[attributeIndex] = value;
            else baseValues[attributeIndex] += value;
            attributesStored = true;
        }

        public void StoreDamage(float3x3 dmg)
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
        public float3x3 inputDmg;
        public int2 inputDmgIndex;
#endif
    }

    public partial struct AilmentRootRuntimeData
    {
        public int stackGroupId;
        public float duration;
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
            public float duration;
            public int maxStacks;
            public int applyStacks;
            public StackMode stackMode;
            public int stackGroupId;
            // public UnityObjectRef<Sprite> icon;
            public LocalizedStringTableReferenceBaked title;
            public LocalizedStringTableReferenceBaked description;
        }

        public struct PolyData
        {
            public int3x3 intMatrix;
            public int2 int2;
            public AttributeShortList attributes;
            public int i1;
            public int i2;
            public float f1;
            public float f2;
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
                duration = GetScaled(root.durationScaleMode, root.duration, (int)ctx.attributes.GetCurrent(root.scaleDurationAttributeIndex)),
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConstructAll<T>(this T ailments, ref AilmentCreationContext ctx, DynamicBuffer<ApplyAilment> to) where T : INativeList<AilmentElement>
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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConstructAll<T>(this T ailments, ref AilmentCreationContext ctx, NativeList<ApplyAilment> to) where T : INativeList<AilmentElement>
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
        public BlobAssetReference<AilmentBlob> blob;
        public int id;
        public int counter;
    }

    [InternalBufferCapacity(0)]
    public partial struct ApplyAilment : IBufferElementData
    {
        public AilmentRuntime ailmentRuntime;
    }
}