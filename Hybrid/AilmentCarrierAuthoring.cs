using GameReady.Ailments.Runtime;
using Src.PackageCandidate.Ailments.Runtime;
using Unity.Entities;
using UnityEngine;

namespace Src.PackageCandidate.Ailments.Hybrid
{
    public class AilmentCarrierAuthoring : MonoBehaviour
    {
        private class AilmentCarrierBaker : Baker<AilmentCarrierAuthoring>
        {
            public override void Bake(AilmentCarrierAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.None);
                AddComponent<AilmentCarrier>(e);
                AddBuffer<ApplyAilment>(e);
                AddBuffer<ActiveAilmentArrayMap>(e);
                AddBuffer<AilmentRuntime>(e);
            }
        }
    }
}