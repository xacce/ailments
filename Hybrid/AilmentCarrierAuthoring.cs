using GameReady.Ailments.Runtime;
using Unity.Entities;
using UnityEngine;

namespace GameReady.Ailments.Hybrid
{
    public class AilmentCarrierAuthoring : MonoBehaviour
    {
        private class AilmentCarrierBaker : Baker<AilmentCarrierAuthoring>
        {
            public override void Bake(AilmentCarrierAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.None);
                AddComponent<AilmentCarrier>(e);
                AddBuffer<ActiveAilmentCounter>(e);
                AddBuffer<ApplyAilment>(e);
                AddBuffer<ConstructedAilment>(e);
            }
        }
    }
}