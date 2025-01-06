using System.Collections.Generic;
using GameReady.Ailments.Runtime;
using Src.PackageCandidate.Attributer;
using Trove.PolymorphicStructs;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Localization.Settings;

namespace Src.PackageCandidate.Ailments.Runtime
{
    [PolymorphicStructInterface]
    public interface IAilment
    {
        public bool Validate(ref Src.PackageCandidate.Ailments.Runtime.AilmentCreationContext ctx, BlobAssetReference<Src.PackageCandidate.Ailments.Runtime.AilmentBlob> blob);

        public AilmentRuntime Create(ref Src.PackageCandidate.Ailments.Runtime.AilmentCreationContext ctx,
            BlobAssetReference<Src.PackageCandidate.Ailments.Runtime.AilmentBlob> blob);

        public void OnFresh(ref Src.PackageCandidate.Ailments.Runtime.AilmentCreatedContext ctx, in Src.PackageCandidate.Ailments.Runtime.AilmentRuntime rootRuntimeData);
        public void OnTick(ref Src.PackageCandidate.Ailments.Runtime.AilmentCreatedContext ctx, in Src.PackageCandidate.Ailments.Runtime.AilmentRuntime rootRuntimeData);
        public void OnExpired(ref Src.PackageCandidate.Ailments.Runtime.AilmentCreatedContext ctx, in Src.PackageCandidate.Ailments.Runtime.AilmentRuntime rootRuntimeData);

        public string Description(ref Src.PackageCandidate.Ailments.Runtime.AilmentCreationContext ctx,
            BlobAssetReference<Src.PackageCandidate.Ailments.Runtime.AilmentBlob> blobReference);

        public string Title(BlobAssetReference<Src.PackageCandidate.Ailments.Runtime.AilmentBlob> blobReference);
    }

    public static class AilmentListExtension
    {
        public static List<string> ToStringArray(this INativeList<AilmentElement> elements, ref AilmentCreationContext ctx)
        {
            var result = new List<string>();
            for (int i = 0; i < elements.Length; i++)
            {
                result.Add(elements[i].ailment.Description(ref ctx, elements[i].blob));
            }

            return result;
        }
    }

    [PolymorphicStruct]
    public partial struct AffectAttributesAilment : IAilment
    {
        public string Description(ref AilmentCreationContext ctx, BlobAssetReference<AilmentBlob> blobReference)
        {
            ref var ailmentBlob = ref blobReference.Value;
            return LocalizationSettings.StringDatabase.GetLocalizedString(
                ailmentBlob.root.description.table,
                ailmentBlob.root.description.key, new List<object>()
                {
                    Create(ref ctx, blobReference).rootRuntimeData,
                    ailmentBlob.root.applyValidationRandomAttribute == -1 ? 100 : ctx.attributes.GetCurrent(ailmentBlob.root.applyValidationRandomAttribute)
                });
        }

        public string Title(BlobAssetReference<AilmentBlob> blobReference)
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(blobReference.Value.root.title.table, blobReference.Value.root.title.key);
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
    public partial struct RawDmgAilment : IAilment
    {
        public float3x3 damage;
        public string Title(BlobAssetReference<AilmentBlob> blobReference)
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(blobReference.Value.root.title.table, blobReference.Value.root.title.key);
        }
        public string Description(ref AilmentCreationContext ctx, BlobAssetReference<AilmentBlob> blobReference)
        {
            ref var ailmentBlob = ref blobReference.Value;
            return LocalizationSettings.StringDatabase.GetLocalizedString(
                ailmentBlob.root.description.table,
                ailmentBlob.root.description.key, new List<object>()
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
            var baseDmg = new float3x3();
            baseDmg[blobValue.polyData.int2.x][blobValue.polyData.int2.y] = blobValue.polyData.f1;

            var ailment = new RawDmgAilment()
            {
                damage = baseDmg,
            };

            return new AilmentRuntime()
            {
                rootRuntimeData = blobValue.Create(ctx),
                blob = blob,
                ailment = ailment
            };
        }

        public void OnFresh(ref AilmentCreatedContext ctx, in AilmentRuntime runtime)
        {
        }

        public void OnTick(ref AilmentCreatedContext ctx, in AilmentRuntime runtime)
        {
            ctx.StoreDamage(((RawDmgAilment)runtime.ailment).damage * ctx.deltaTime);
        }

        public void OnExpired(ref AilmentCreatedContext ctx, in AilmentRuntime runtime)
        {
        }
    }

    [PolymorphicStruct]
    public partial struct AddTagAilment : IAilment
    {
        public int tag;
        public string Title(BlobAssetReference<AilmentBlob> blobReference)
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(blobReference.Value.root.title.table, blobReference.Value.root.title.key);
        }
        public string Description(ref AilmentCreationContext ctx, BlobAssetReference<AilmentBlob> blobReference)
        {
            ref var ailmentBlob = ref blobReference.Value;
            return LocalizationSettings.StringDatabase.GetLocalizedString(
                ailmentBlob.root.description.table,
                ailmentBlob.root.description.key, new List<object>()
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