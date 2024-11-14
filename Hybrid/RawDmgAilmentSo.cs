#if DAMAGE_MODULE
using System;
using GameReady.Ailments.Runtime;
using Src.PackageCandidate.Attributer;
using Trove.PolymorphicStructs;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Src.PackageCandidate.GameReady.Ailments.Hybrid
{
    [PolymorphicStruct]
    [Serializable]
    public partial struct RawDmgAilment : IAilment
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
    public partial struct RawDmgAilmentConstructor : IAilmentConstructor
    {
        public int3x3 dmg;
        public DamageAttributeScaleMap scaleMap;
        public AilmentRootConstructor constructor;


        public bool TryConstruct(AilmentPreConstructedContext ctx, out ConstructedAilment constructed)
        {
            var baseDmg = dmg;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    baseDmg[i][j] = (int)AttributesHelper.ApplyAddictiveSafe(baseDmg[i][j], ctx.attributes.GetCurrent(scaleMap.attributes[i][j]));
                }
            }

            var ailment = new RawDmgAilment()
            {
                dmg = baseDmg,
            };
            constructed= new ConstructedAilment
            {
                ailment = ailment,
                root = constructor.Construct(ctx)
            };
            return true;
        }
    }


    [CreateAssetMenu(menuName = "Sufferenger/Ailments/Raw dmg ailment")]
    public class RawDmgAilmentSo : AilmentBakedSo
    {
        [SerializeField] private DamageAttributeScaleMapSo scale;
        [SerializeField] private RawDmgAilmentConstructor node;

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
    }
}
#endif