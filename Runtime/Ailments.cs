using GameReady.Ailments.Runtime;
using Trove.PolymorphicStructs;
using Unity.Entities;
using UnityEngine;

namespace Src.PackageCandidate.Ailments.Runtime
{
    [PolymorphicStructInterface]
    public interface IAilment
    {
        public AilmentRuntime Create(ref AilmentCreationContext ctx,BlobAssetReference<AilmentBlob> blob);
        public void OnFresh(ref AilmentCreatedContext ctx,in AilmentRuntime rootRuntimeData);
        public void OnTick(ref AilmentCreatedContext ctx,in AilmentRuntime rootRuntimeData);
        public void OnExpired(ref AilmentCreatedContext ctx,in AilmentRuntime rootRuntimeData);
    }
    
    [PolymorphicStruct]
    public partial struct AffectAttributesAilment : IAilment
    {
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