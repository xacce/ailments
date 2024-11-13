using System;
using Src.PackageCandidate.GameReady.Ailments.Runtime;
using Unity.Entities;
using UnityEngine;

namespace Src.PackageCandidate.GameReady.Ailments.Hybrid
{
    public class DamageAilmentInflictAuthoring : MonoBehaviour
    {
        [Tooltip("Релизует дебафы от урона")] [SerializeField] private DamageAilmentInflictDefaultCollectionSo[] collections = Array.Empty<DamageAilmentInflictDefaultCollectionSo>();

        private class DamageAilmentInflictBaker : Baker<DamageAilmentInflictAuthoring>
        {
            public override void Bake(DamageAilmentInflictAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.None);
                var buffer = AddBuffer<DamageAilmentInflict>(e);
                foreach (var collection in authoring.collections)
                {
                    collection.Bake(buffer, this);
                }
            }
        }
    }
}