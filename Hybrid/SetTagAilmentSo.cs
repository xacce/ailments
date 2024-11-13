using System;
using Core.Hybrid;
using GameReady.Ailments.Runtime;
using Trove.PolymorphicStructs;
using Unity.Entities;
using UnityEditor.Experimental.GraphView;
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
        public abstract AilmentConstructor BakeAilment(IBaker blobBuilder);
        public abstract void BakeAilment(ref BlobBuilder blobBuilder, ref AilmentConstructor constructor);
        public abstract int id { get; }
    }

    [CreateAssetMenu(menuName = "Sufferenger/Ailments/Set tag ailment")]
    public class SetTagAilmentSo : AilmentBakedSo
    {
        [SerializeField] private AddTagAilmentConstructor node;

        public override AilmentConstructor BakeAilment(IBaker blobBuilder)
        {
            return node;
        }

        public override void BakeAilment(ref BlobBuilder blobBuilder, ref AilmentConstructor constructor)
        {
            constructor = node;
        }

        public override int id => node.constructor.stackGroupId;
    }
}