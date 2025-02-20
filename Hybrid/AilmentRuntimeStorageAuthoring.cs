using Src.PackageCandidate.Ailments.Runtime;
using Unity.Entities;
using UnityEngine;

namespace Src.PackageCandidate.Ailments.Hybrid
{
    public class AilmentRuntimeStorageAuthoring : MonoBehaviour
    {
        class _ : Baker<AilmentRuntimeStorageAuthoring>
        {
            public override void Bake(AilmentRuntimeStorageAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.None);
                AddBuffer<ApplyAilment>(e);
                // AddComponent(new AilmentRuntimeStorageAuthoring{},e);
                // AddBuffer<AilmentRuntimeStorageAuthoring>(e)
            }
        }
    }
}