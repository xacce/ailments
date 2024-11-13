using System;
using Src.PackageCandidate.GameReady.Ailments.Runtime;
using Unity.Entities;
using UnityEngine;

namespace Src.PackageCandidate.GameReady.Ailments.Hybrid
{
    [CreateAssetMenu(menuName = "Sufferenger/Ailments/Base damage inflict ailments collection")]
    public class DamageAilmentInflictDefaultCollectionSo : ScriptableObject
    {
        [SerializeField] private DamageAilmentInflictSo[] ailments = Array.Empty<DamageAilmentInflictSo>();

        public void Bake(DynamicBuffer<DamageAilmentInflict> buffer, IBaker baker)
        {
            foreach (var ailment in ailments)
            {
                buffer.Add(new DamageAilmentInflict() { blob = ailment.Bake(baker) });
            }
        }
    }
}