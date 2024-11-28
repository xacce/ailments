using Src.PackageCandidate.Ailments.Runtime;
using Src.PackageCandidate.Attributer;
using Trove.PolymorphicStructs;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

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
        [ReadOnly] public DynamicBuffer<AttributeValue> attributes;
#if DAMAGE_MODULE
        public int3x3 inputDmg;
        public int2 inputDmgIndex;
#endif
    }

    public partial struct AilmentRootRuntimeData
    {
        public int stackGroupId;
        public uint duration;
        public uint maxStacks;
        public uint split;
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
            public ScaleMode applySacksScaleMode;
            public uint durationTicks;
            public uint maxStacks;
            public uint applyStacks;
            public StackMode stackMode;
            public int stackGroupId;
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

        public Root root;
        public PolyData polyData;


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

        public AilmentRootRuntimeData Create(AilmentCreationContext ctx)
        {
            return new AilmentRootRuntimeData()
            {
                duration = GetScaled(root.durationScaleMode, root.durationTicks, (uint)ctx.attributes.GetCurrent(root.scaleDurationAttributeIndex)),
                maxStacks = GetScaled(root.maxStacksScaleMode, root.maxStacks, (uint)ctx.attributes.GetCurrent(root.scaleMaxStacksAttributeIndex)),
                stackGroupId = root.stackGroupId,
                stackMode = root.stackMode,
                split = GetScaled(root.applySacksScaleMode, root.applyStacks, (uint)ctx.attributes.GetCurrent(root.applyStacksAttributeIndex)),
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