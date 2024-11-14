using System;
using Core.Hybrid;
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
    public partial struct AffectAttributesAilment : IAilment
    {
        public int4 attrs;
        public int len;
        public float4 values;

        public void OnFresh(ref AilmentConstructedContext ctx)
        {
            for (int i = 0; i < math.max(len, 3); i++)
            {
                ctx.AddBaseAttributeValue(attrs[i], values[i]);
            }
        }

        public void OnTick(ref AilmentConstructedContext ctx)
        {
        }

        public void OnExpired(ref AilmentConstructedContext ctx)
        {
            for (int i = 0; i < math.max(len, 3); i++)
            {
                ctx.AddBaseAttributeValue(attrs[i], -values[i]);
            }
        }
    }

    [PolymorphicStruct]
    public partial struct AffectAttributesAilmentConstructor : IAilmentConstructor
    {
        public int4 attrs;
        public float4 values;
        public int len;
        public AilmentRootConstructor root;


        public bool TryConstruct(AilmentPreConstructedContext ctx, out ConstructedAilment constructed)
        {
            var ailment = new AffectAttributesAilment()
            {
                attrs = attrs,
                values = values,
                len = len,
            };
            constructed = new ConstructedAilment() { ailment = ailment, root = root.Construct(ctx) };
            return true;
        }
    }


    [CreateAssetMenu(menuName = "Sufferenger/Ailments/Affect attrs ailment")]
    public class AffectAttributesAilmentSo : AilmentBakedSo
    {
        [SerializeField] private AttributeValuesPresetSo affectAttributes;
        [SerializeField] private AilmentRootConstructorBaked root;


        public override void BakeAilment(ref BlobBuilder blobBuilder, ref AilmentConstructor constructor)
        {
            var vals = affectAttributes.GetFilledValues();
            var affect = new AffectAttributesAilmentConstructor();
            for (int i = 0; i < vals.Length; i++)
            {
                affect.attrs[i] = vals[i].attribute.id;
                affect.values[i] = vals[i].value;
            }

            affect.len = vals.Length;
            constructor = affect;
            constructor.AilmentRootConstructor_1 = root.Bake();
        }

        public override int id => root.stackGroupId;
    }
}