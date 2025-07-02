using System.Collections.Generic;
using GameReady.Ailments.Runtime;
using Src.PackageCandidate.Attributer;
using Src.PackageCandidate.LogTest;
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
        public static string[] ToStringArray(this INativeList<AilmentElement> elements, ref AilmentCreationContext ctx,AilmentDatabaseSingleton db)
        {
            var result = new string[elements.Length];
            for (int i = 0; i < elements.Length; i++)
            {
                if(!db.TryGetById(elements[i].id, out var ailment)) continue;
                result[i] = ailment.ailment.Description(ref ctx, ailment.blob);
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
                rootRuntimeData = blobValue.Create(ctx, blobValue.polyData.value),
                blob = blob,
                ailment = new AffectAttributesAilment { }
            };
        }

        public void OnFresh(ref AilmentCreatedContext ctx, in AilmentRuntime runtime)
        {
            ref var blob = ref runtime.blob.Value;
            GameDebug.Spam("Ailment", $"On fresh for AffectAttributesAilment[{runtime.blob.Value.root.stackGroupId}]");
            GameDebug.Spam("Ailment",
                $"- Effective: {runtime.rootRuntimeData.effectivity}, duration: {runtime.rootRuntimeData.duration}, max stacks: {runtime.rootRuntimeData.maxStacks}, weight value: {runtime.rootRuntimeData.value}");
            var effectivity = runtime.rootRuntimeData.effectivity;
            for (int i = 0; i < blob.polyData.attributes.list.Length; i++)
            {
                var value = blob.polyData.attributes.list[i].value;
                value *= effectivity;
                GameDebug.Spam("Ailment", $"- Attribute {blob.polyData.attributes.list[i].index} value: {value} (origin: {blob.polyData.attributes.list[i].value}");
                ctx.AddBaseAttributeValue(blob.polyData.attributes.list[i].index, value);
            }
        }

        public void OnTick(ref AilmentCreatedContext ctx, in AilmentRuntime runtime)
        {
        }

        public void OnExpired(ref AilmentCreatedContext ctx, in AilmentRuntime runtime)
        {
            ref var blob = ref runtime.blob.Value;
            var effectivity = runtime.rootRuntimeData.effectivity;
            GameDebug.Spam("Ailment", $"On expired for AffectAttributesAilment[{runtime.blob.Value.root.stackGroupId}], effectivity: {effectivity}");

            for (int i = 0; i < blob.polyData.attributes.list.Length; i++)
            {
                var value = blob.polyData.attributes.list[i].value * effectivity;
                GameDebug.Spam("Ailment", $"- Attribute {blob.polyData.attributes.list[i].index} revert value : {value} (origin: {blob.polyData.attributes.list[i].value}");
                ctx.AddBaseAttributeValue(blob.polyData.attributes.list[i].index, -value);
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

            var s = (int3)ailment.damage.c0;
            return new AilmentRuntime()
            {
                rootRuntimeData = blobValue.Create(ctx, blobValue.polyData.value),
                blob = blob,
                ailment = ailment
            };
        }

        public void OnFresh(ref AilmentCreatedContext ctx, in AilmentRuntime runtime)
        {
            GameDebug.Spam("Ailment", $"On fresh for RawDmgAilment[{runtime.blob.Value.root.stackGroupId}], effectivity: {runtime.rootRuntimeData.effectivity}");
        }

        public void OnTick(ref AilmentCreatedContext ctx, in AilmentRuntime runtime)
        {
            GameDebug.Spam("Ailment", $"On tick for RawDmgAilment[{runtime.blob.Value.root.stackGroupId}], effectivity: {runtime.rootRuntimeData.effectivity}");
            ctx.StoreDamage(((RawDmgAilment)runtime.ailment).damage * (runtime.rootRuntimeData.effectivity * ctx.deltaTime));
        }

        public void OnExpired(ref AilmentCreatedContext ctx, in AilmentRuntime runtime)
        {
            GameDebug.Spam("Ailment", $"On expired for RawDmgAilment[{runtime.blob.Value.root.stackGroupId}], effectivity: {runtime.rootRuntimeData.effectivity}");
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
                rootRuntimeData = blobValue.Create(ctx, blobValue.polyData.value),
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