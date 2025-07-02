using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Core.Runtime;
using Core.Runtime.LatiosHashMap.Latios;
using GameReady.Ailments.Runtime;
using Src.PackageCandidate.Attributer;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
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
        public byte dmgStored;
        public byte attributesStored;

        public void AddBaseAttributeValue(int attributeIndex, float value)
        {
            if (!baseValues.ContainsKey(attributeIndex)) baseValues[attributeIndex] = value;
            else baseValues[attributeIndex] += value;
            attributesStored = 1;
        }

        public void StoreDamage(float3x3 dmg)
        {
            accomulatedDmg += dmg;
            dmgStored = 1;
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

    public partial struct AilmentDatabaseSingleton : IComponentData
    {
        [ReadOnly] public UnsafeHashMap<int, AilmentElementRegistry> database;

        public bool TryGetById(int id, out AilmentElementRegistry item)
        {
            return database.TryGetValue(id, out item);
        }
    }

    [InternalBufferCapacity(0)]
    public partial struct AilmentElementRegistry : IBufferElementData
    {
        public BlobAssetReference<AilmentBlob> blob;
        public Ailment ailment;
        public int id;
    }

    public partial struct AilmentRootRuntimeData
    {
        public int value;
        public int stackGroupId;
        public float duration;
        public float effectivity;
        public int maxStacks;
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

            public int effectivityAttributeIndex;
            public ScaleMode effectivityScaleMode;

            public int defensiveEffectivityAttributeIndex;
            public ScaleMode defensiveEffectivityMode;

            public float duration;
            public int maxStacks;
            public float baseEffectivity;
            public int applyStacks;
            public StackMode stackMode;

            public int stackGroupId;
            public BlobString style;
            public LocalizedStringTableReferenceBaked title;
            public LocalizedStringTableReferenceBaked description;
            public LocalizedStringTableReferenceBaked tooltipDescription;
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
            public int value;
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
                    return math.max(AttributesHelper.ApplyAddictive(origin, mod), 0);
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
            data.duration = root.defensiveScaleDurationAttributeIndex > 0
                ? GetScaled(root.defensiveScaleDurationMode, data.duration, attrs.GetCurrent(root.defensiveScaleDurationAttributeIndex))
                : data.duration;

            data.maxStacks = root.defensiveScaleMaxStacksAttributeIndex > 0
                ? GetScaled(root.defensiveScaleMaxStacksMode, data.maxStacks, (int)attrs.GetCurrent(root.defensiveScaleMaxStacksAttributeIndex))
                : data.maxStacks;

            data.effectivity = root.defensiveEffectivityAttributeIndex > 0
                ? GetScaled(root.defensiveEffectivityMode, data.effectivity, attrs.GetCurrent(root.defensiveEffectivityAttributeIndex))
                : data.effectivity;
        }

        public AilmentRootRuntimeData Create(AilmentCreationContext ctx, int value)
        {
            return new AilmentRootRuntimeData()
            {
                duration = root.scaleDurationAttributeIndex > 0
                    ? GetScaled(root.durationScaleMode, root.duration, ctx.attributes.GetCurrent(root.scaleDurationAttributeIndex))
                    : root.duration,

                maxStacks = root.scaleMaxStacksAttributeIndex > 0
                    ? GetScaled(root.maxStacksScaleMode, root.maxStacks, (int)ctx.attributes.GetCurrent(root.scaleMaxStacksAttributeIndex))
                    : root.maxStacks,

                effectivity = root.effectivityAttributeIndex > 0
                    ? GetScaled(root.effectivityScaleMode, root.baseEffectivity, ctx.attributes.GetCurrent(root.effectivityAttributeIndex))
                    : root.baseEffectivity,

                stackGroupId = root.stackGroupId,
                value = value,
            };
        }
    }

    [InternalBufferCapacity(0)]
    public partial struct AilmentRuntime : IBufferElementData
    {
        public BlobAssetReference<AilmentBlob> blob;
        public AilmentRootRuntimeData rootRuntimeData;
        public Ailment ailment;

        public void Clear()
        {
            rootRuntimeData.duration = 0;
        }
    }

    [InternalBufferCapacity(0)]
    public partial struct AilmentElement : IBufferElementData
    {
        // public BlobAssetReference<AilmentBlob> blob;
        public int id;
    }

    public static class AilmentElementExtender
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConstructAll<T>(this T ailments, ref AilmentCreationContext ctx, AilmentDatabaseSingleton db, DynamicBuffer<ApplyAilment> to)
            where T : INativeList<AilmentElement>
        {
            for (int i = 0; i < ailments.Length; i++)
            {
                if (!db.TryGetById(ailments[i].id, out var ailment)) continue;
                if (ailment.ailment.Validate(ref ctx, ailment.blob))
                {
                    to.Add(new ApplyAilment() { ailmentRuntime = ailment.ailment.Create(ref ctx, ailment.blob) });
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConstructAll<T>(this T ailments, ref AilmentCreationContext ctx, AilmentDatabaseSingleton db, NativeList<ApplyAilment> to)
            where T : INativeList<AilmentElement>
        {
            for (int i = 0; i < ailments.Length; i++)
            {
                if (!db.TryGetById(ailments[i].id, out var ailment)) continue;
                if (ailment.ailment.Validate(ref ctx, ailment.blob))
                {
                    to.Add(new ApplyAilment() { ailmentRuntime = ailment.ailment.Create(ref ctx, ailment.blob) });
                }
            }
        }
    }

    [Serializable]
    public struct AilmentInfo : IEquatable<AilmentInfo>
    {
        public int stacksCount;
        public float duration;
        public BlobAssetReference<AilmentBlob> blob;

        public bool Equals(AilmentInfo other)
        {
            return blob.Value.root.stackGroupId.Equals(other.blob.Value.root.stackGroupId);
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            return blob.GetHashCode();
        }
    }

    [InternalBufferCapacity(0)]
    public partial struct ActiveAilmentArrayMap : IBufferElementData
    {
        public DynamicHashMap<int, AilmentInfo>.Pair pair;
    }


    [InternalBufferCapacity(0)]
    public partial struct ApplyAilment : IBufferElementData
    {
        public AilmentRuntime ailmentRuntime;
    }
}