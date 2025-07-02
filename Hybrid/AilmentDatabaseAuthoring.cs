#if UNITY_EDITOR
using System.Collections.Generic;
using GameReady.Ailments.Runtime;
using Src.PackageCandidate.Ailments.Runtime;
using Src.PackageCandidate.GameReady.Ailments.Hybrid;
using Src.PackageCandidate.PrefabCollection;
using Src.PackageCandidate.PrefabCollection.Authoring;
using Unity.Entities;
using UnityEngine;

namespace GameReady.Ailments.Hybrid
{
    public class AilmentDatabaseAuthoring : MonoBehaviour
    {
        private class _ : Baker<AilmentDatabaseAuthoring>
        {
            public override void Bake(AilmentDatabaseAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.None);
                var items = AddBuffer<AilmentElementRegistry>(e);
                var hs = new HashSet<int>();
                foreach (var item in AilmentDatabaseEditorSingleton.instance.items)
                {
                    if (!hs.Add(item.id))
                    {
                        Debug.LogError($"Prefab with {item.id} already baked, duplicate or u are idiot?. Check so: {item}", item);
                        continue;
                    }

                    items.Add(new AilmentElementRegistry()
                    {
                        ailment = item.ailment,
                        blob = item.Bake(this),
                        id = item.id,
                    });
                }

                Debug.Log($"Bake {items.Length} prefabs");
            }
        }
    }
}
#endif