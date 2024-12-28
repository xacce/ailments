#if DAMAGE_MODULE
using System;
using GameReady.Ailments.Runtime;
using Src.PackageCandidate.Ailments.Hybrid;
using Src.PackageCandidate.Ailments.Runtime;
using Src.PackageCandidate.Attributer;
using Src.PackageCandidate.Sufferenger.Authoring;
using Sufferenger;
using Trove.PolymorphicStructs;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Src.PackageCandidate.GameReady.Ailments.Hybrid
{
    [PolymorphicStruct]
    [Serializable]
    public partial struct InputDmgAilment : IAilment
    {
        public int3x3 dmg;

        public void OnFresh(ref AilmentConstructedContext ctx)
        {
        }

        public void OnTick(ref AilmentConstructedContext ctx)
        {
            ctx.StoreDamage(dmg);
        }

        public void OnExpired(ref AilmentConstructedContext ctx)
        {
        }
    }

    [PolymorphicStruct]
    [Serializable]
    public partial struct InputDmgAilmentConstructor : IAilmentConstructor
    {
        public DamageAttributeScaleMap scaleMap;
        public AilmentRootConstructor constructor;
        [Range(0.001f, 2f)] [SerializeField] public float percent;

        public bool TryConstruct(AilmentPreConstructedContext ctx, out ConstructedAilment constructed)
        {
            var baseDmg = new int3x3();
            var x = ctx.inputDmgIndex.x;
            var y = ctx.inputDmgIndex.y;
            var cDmg = ctx.inputDmg[x][y] * percent;
            constructed = default;
            if (cDmg <= 0) return false;

            var outdmg = (int)AttributesHelper.ApplyAddictiveSafe(cDmg, ctx.attributes.GetCurrent(scaleMap.attributes[x][y]));
            baseDmg[x][y] = outdmg;
            var ailment = new InputDmgAilment()
            {
                dmg = baseDmg,
            };
            constructed = new ConstructedAilment
            {
                ailment = ailment,
                root = constructor.Construct(ctx)
            };
            return true;
        }
    }


    [CreateAssetMenu(menuName = "Sufferenger/Ailments/Input dmg ailment")]
    public class InputDmgAilmentSo : AilmentBakedSo
    {
        [SerializeField] private DamageAttributeScaleMapSo scale;
        [SerializeField] private InputDmgAilmentConstructor node;

        public override AilmentConstructor BakeAilment(IBaker blobBuilder)
        {
            var rnode = node;
            rnode.scaleMap = scale;
            return rnode;
        }

        public override void BakeAilment(ref BlobBuilder blobBuilder, ref AilmentConstructor constructor)
        {
            var rnode = node;
            rnode.scaleMap = scale;
            constructor = rnode;
        }

        public override int id => node.constructor.stackGroupId;
        public override Ailment ailment { get; }

        public override void Bake(ref AilmentBlob data, ref BlobBuilder blobBuilder)
        {
            throw new NotImplementedException();
        }
    }
}
#endif