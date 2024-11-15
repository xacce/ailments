using System;
using Core.Hybrid;
using Src.PackageCandidate.Ailments.Hybrid;
using Src.PackageCandidate.Attributer.Authoring;

namespace GameReady.Ailments.Runtime
{
    [Serializable]
    public partial struct AilmentBlobRootBaked
    {
        public AttributeSo scaleDurationAttributeIndex;
        public AilmentBlob.ScaleMode durationScaleMode;
        public AttributeSo scaleMaxStacksAttributeIndex;
        public AilmentBlob.ScaleMode maxStacksScaleMode;
        public AttributeSo applyStacksAttributeIndex;
        public AilmentBlob.ScaleMode applySacksScaleMode;
        public uint durationTicks;
        public uint maxStacks;
        public uint applyStacks;
        public StackMode stackMode;

        [PickId(typeof(AilmentBakedSo))] public int stackGroupId;

        public AilmentBlob.Root Bake()
        {
            return new AilmentBlob.Root()
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
}