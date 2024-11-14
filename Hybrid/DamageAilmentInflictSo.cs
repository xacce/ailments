#if DAMAGE_MODULE
using System;
using System.Collections.Generic;
using Core.Hybrid;
using GameReady.Ailments.Runtime;
using Src.PackageCandidate.GameReady.Ailments.Runtime;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Src.PackageCandidate.GameReady.Ailments.Hybrid
{
    [CreateAssetMenu(menuName = "Sufferenger/Ailments/Base damage inflict ailments")]
    public class DamageAilmentInflictSo : BakedScriptableObject<DamageAilmentInflict.Blob>
    {
        [Serializable]
        internal struct WithInt3x3Index
        {
            [SerializeField] internal AilmentBakedSo ailment;
            [SerializeField] internal int2 dmgIndex;
        }

        [SerializeField] private WithInt3x3Index[] ailments = Array.Empty<WithInt3x3Index>();
        [SerializeField] private DamageAttributeScaleMapSo chancesMap;

        public override void Bake(ref DamageAilmentInflict.Blob data, ref BlobBuilder blobBuilder)
        {
            int i = 0;
            List<AilmentBakedSo> onlyails = new List<AilmentBakedSo>();
            data.indexes = -1;
            data.chancesMap = chancesMap;
            foreach (var related in ailments)
            {
                data.indexes[related.dmgIndex.x][related.dmgIndex.y] = i;
                onlyails.Add(related.ailment);
                i++;
            }

            AilmentConstructors.Create(ref data.constructors, ref blobBuilder, onlyails.ToArray());
        }
    }
}
#endif