using System.Collections.Generic;
using System.Text;
using GameReady.Ailments.Runtime;
using Src.PackageCandidate.Attributer;
using Trove.PolymorphicStructs;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.TextCore.Text;

namespace Src.PackageCandidate.Ailments.Runtime
{
    [PolymorphicStructInterface]
    public interface IAilment
    {
        public bool Validate(ref AilmentCreationContext ctx, BlobAssetReference<AilmentBlob> blob);
        public AilmentRuntime Create(ref AilmentCreationContext ctx, BlobAssetReference<AilmentBlob> blob);
        public void OnFresh(ref AilmentCreatedContext ctx, in AilmentRuntime rootRuntimeData);
        public void OnTick(ref AilmentCreatedContext ctx, in AilmentRuntime rootRuntimeData);
        public void OnExpired(ref AilmentCreatedContext ctx, in AilmentRuntime rootRuntimeData);
        public string ToString(ref AilmentCreationContext ctx, BlobAssetReference<AilmentBlob> blobReference);
    }

    public static class AilmentListExtension
    {
        public static List<string> ToStringArray(this INativeList<AilmentElement> elements, ref AilmentCreationContext ctx)
        {
            var result = new List<string>();
            for (int i = 0; i < elements.Length; i++)
            {
                result.Add(elements[i].ailment.ToString(ref ctx, elements[i].blob));
            }

            return result;
        }
    }

    [PolymorphicStruct]
    public partial struct AffectAttributesAilment : IAilment
    {
        public string ToString(ref AilmentCreationContext ctx, BlobAssetReference<AilmentBlob> blobReference)
        {
            ref var ailmentBlob = ref blobReference.Value;
            return LocalizationSettings.StringDatabase.GetLocalizedString(
                ailmentBlob.root.title.table,
                ailmentBlob.root.title.key, new List<object>()
                {
                    Create(ref ctx, blobReference).rootRuntimeData,
                    ailmentBlob.root.applyValidationRandomAttribute == -1 ? 100 : ctx.attributes.GetCurrent(ailmentBlob.root.applyValidationRandomAttribute)
                });
        }

        public bool Validate(ref AilmentCreationContext ctx, BlobAssetReference<AilmentBlob> blob)
        {
            return AilmentBlob.Validate(ref ctx, blob);
        }

        public AilmentRuntime Create(ref AilmentCreationContext ctx, BlobAssetReference<AilmentBlob> blob)
        {
            ref var blobValue = ref blob.Value;

            return new AilmentRuntime()
            {
                rootRuntimeData = blobValue.Create(ctx),
                blob = blob,
                ailment = new AffectAttributesAilment { }
            };
        }

        public void OnFresh(ref AilmentCreatedContext ctx, in AilmentRuntime runtime)
        {
            ref var blob = ref runtime.blob.Value;
            for (int i = 0; i < blob.polyData.attributes.list.Length; i++)
            {
                ctx.AddBaseAttributeValue(blob.polyData.attributes.list[i].index, blob.polyData.attributes.list[i].value);
            }
        }

        public void OnTick(ref AilmentCreatedContext ctx, in AilmentRuntime runtime)
        {
        }

        public void OnExpired(ref AilmentCreatedContext ctx, in AilmentRuntime runtime)
        {
            ref var blob = ref runtime.blob.Value;
            for (int i = 0; i < blob.polyData.attributes.list.Length; i++)
            {
                ctx.AddBaseAttributeValue(blob.polyData.attributes.list[i].index, -blob.polyData.attributes.list[i].value);
            }
        }
    }


    [PolymorphicStruct]
    public partial struct AddTagAilment : IAilment
    {
        public int tag;

        public string ToString(ref AilmentCreationContext ctx, BlobAssetReference<AilmentBlob> blobReference)
        {
            ref var ailmentBlob = ref blobReference.Value;
            return LocalizationSettings.StringDatabase.GetLocalizedString(
                ailmentBlob.root.title.table,
                ailmentBlob.root.title.key, new List<object>()
                {
                    Create(ref ctx, blobReference).rootRuntimeData,
                    ailmentBlob.root.applyValidationRandomAttribute == -1 ? 100 : ctx.attributes.GetCurrent(ailmentBlob.root.applyValidationRandomAttribute)
                });
        }

        public bool Validate(ref AilmentCreationContext ctx, BlobAssetReference<AilmentBlob> blob)
        {
            return AilmentBlob.Validate(ref ctx, blob);
        }

        public AilmentRuntime Create(ref AilmentCreationContext ctx, BlobAssetReference<AilmentBlob> blob)
        {
            ref var blobValue = ref blob.Value;

            return new AilmentRuntime()
            {
                rootRuntimeData = blobValue.Create(ctx),
                blob = blob,
                ailment = new AddTagAilment { tag = blobValue.polyData.i1 }
            };
        }

        public void OnFresh(ref AilmentCreatedContext ctx, in AilmentRuntime rootRuntimeData)
        {
            ctx.carrier.tags |= (AilmentTag)tag;
        }

        public void OnTick(ref AilmentCreatedContext ctx, in AilmentRuntime rootRuntimeData)
        {
        }

        public void OnExpired(ref AilmentCreatedContext ctx, in AilmentRuntime rootRuntimeData)
        {
            ctx.carrier.tags &= ~(AilmentTag)tag;
        }
    }
}