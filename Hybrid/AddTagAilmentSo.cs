using System;
using Core.Hybrid;
using GameReady.Ailments.Runtime;
using Trove.PolymorphicStructs;
using Unity.Entities;
using UnityEngine;

namespace Src.PackageCandidate.GameReady.Ailments.Hybrid
{
    [PolymorphicStruct]
    [Serializable]
    public partial struct AddTagAilment : IAilment
    {
        public AilmentTag tag;

        public void OnFresh(ref AilmentConstructedContext ctx)
        {
            ctx.carrier.tags |= tag;
        }

        public void OnTick(ref AilmentConstructedContext ctx)
        {
        }

        public void OnExpired(ref AilmentConstructedContext ctx)
        {
            ctx.carrier.tags &= ~tag;
        }
    }

    [PolymorphicStruct]
    [Serializable]
    public partial struct AddTagAilmentConstructor : IAilmentConstructor
    {
        public AilmentTag tag;
        public AilmentRootConstructor constructor;


        public bool TryConstruct(AilmentPreConstructedContext ctx, out ConstructedAilment constructed)
        {
            var addTagAilment = new AddTagAilment() { tag = tag };
            constructed = new ConstructedAilment() { ailment = addTagAilment, root = constructor.Construct(ctx) };
            return true;
        }
    }

    public abstract class AilmentBakedSo : ScriptableObject,IUniqueIdProvider
    {
        public abstract void BakeAilment(ref BlobBuilder blobBuilder, ref AilmentConstructor constructor);
        public abstract int id { get; }
    }

    [CreateAssetMenu(menuName = "Sufferenger/Ailments/Set tag ailment")]
    public class AddTagAilmentSo : AilmentBakedSo
    {
        [SerializeField] private AddTagAilmentConstructor node;
        [SerializeField] private AilmentRootConstructorBaked root;

     
        public override void BakeAilment(ref BlobBuilder blobBuilder, ref AilmentConstructor constructor)
        {
            var cp= node;
            constructor = cp;
            cp.constructor = root.Bake();
        }

        public override int id => node.constructor.stackGroupId;
    }
}