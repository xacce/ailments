using System;
using Core.Hybrid;
using Src.PackageCandidate.Ailments.Hybrid;
using Src.PackageCandidate.Ailments.Runtime;
using Src.PackageCandidate.Attributer.Authoring;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.Serialization;

namespace GameReady.Ailments.Runtime
{
    [Serializable]
    public partial class AilmentBlobRootBaked
    {
        public AttributeSo scaleDurationAttributeIndex;
        public AilmentBlob.ScaleMode durationScaleMode;
        public AttributeSo scaleMaxStacksAttributeIndex;
        public AilmentBlob.ScaleMode maxStacksScaleMode;
        public AttributeSo applyStacksAttributeIndex;
        public AilmentBlob.ScaleMode applySacksScaleMode;
        public AttributeSo applyValidationRandomAttribute;

        public AttributeSo defensiveScaleDurationAttributeIndex;
        public AilmentBlob.ScaleMode defensiveDurationScaleMode;
        public AttributeSo defensiveScaleMaxStacksAttributeIndex;
        public AilmentBlob.ScaleMode defensiveMaxStacksScaleMode;


        [FormerlySerializedAs("durationTicks")]
        public uint duration;

        public uint maxStacks;
        public uint applyStacks;
        public StackMode stackMode;

        [SerializeField] LocalizedString title = new LocalizedString();
        [SerializeField] LocalizedString description = new LocalizedString();
        [SerializeField] private Sprite icon;

        [PickId(typeof(AilmentBakedSo))] public int stackGroupId;
#if UNITY_EDITOR
        public void Bake(ref BlobBuilder builder, ref AilmentBlob.Root field)
        {
            field = new AilmentBlob.Root()
            {
                stackGroupId = stackGroupId,
                applyStacksAttributeIndex = applyStacksAttributeIndex ? applyStacksAttributeIndex.id : 0,
                scaleDurationAttributeIndex = scaleDurationAttributeIndex ? scaleDurationAttributeIndex.id : 0,
                scaleMaxStacksAttributeIndex = scaleMaxStacksAttributeIndex ? scaleMaxStacksAttributeIndex.id : 0,
                applyValidationRandomAttribute = applyValidationRandomAttribute ? applyValidationRandomAttribute.id : -1,

                defensiveScaleDurationAttributeIndex = defensiveScaleDurationAttributeIndex ? defensiveScaleDurationAttributeIndex.id : 0,
                defensiveScaleMaxStacksAttributeIndex = defensiveScaleMaxStacksAttributeIndex ? defensiveScaleMaxStacksAttributeIndex.id : 0,

                applyStacks = (int)applyStacks,
                duration = (int)duration,
                maxStacks = (int)maxStacks,
                stackMode = stackMode,
                durationScaleMode = durationScaleMode,
                applyStacksScaleMode = applySacksScaleMode,
                maxStacksScaleMode = maxStacksScaleMode,
                defensiveScaleDurationMode = defensiveDurationScaleMode,
                defensiveScaleMaxStacksMode = defensiveMaxStacksScaleMode,
                title = title,
                description = description,
                icon = icon
            };
        }
#endif
    }
}