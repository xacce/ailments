using System;
using GameReady.Ailments.Runtime;
using Src.PackageCandidate.Ailments.Runtime;
using Unity.Entities;
using UnityEngine;

namespace Src.PackageCandidate.Ailments.Hybrid
{
    public class AilmentStorageAuthoring : MonoBehaviour
    {
        [SerializeField] private AilmentBakedSo[] ailments = Array.Empty<AilmentBakedSo>();

        private class AilmentStorageBaker : Baker<AilmentStorageAuthoring>
        {
            public override void Bake(AilmentStorageAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.None);
                var ailments = AddBuffer<AilmentElement>(e);
                foreach (var ailment in authoring.ailments)
                {
                    ailments.Add(new AilmentElement()
                    {
                        ailment = ailment.ailment,
                        blob = ailment.Bake(this)
                    });
                }
            }
        }
    }
}