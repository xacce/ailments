using Src.PackageCandidate.Ailments.Runtime;
using Src.PackageCandidate.DotsItem;
using Src.PackageCandidate.LogTest;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace DotsItem
{
    [BurstCompile]
    partial struct AilmentDatabaseSystem : ISystem
    {
        private bool _initialized;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        public void Initialize(ref SystemState state)
        {
            if (!SystemAPI.HasSingleton<AilmentElementRegistry>()) return;
            _initialized = true;
            var items = SystemAPI.GetSingletonBuffer<AilmentElementRegistry>();
            var ailments = new UnsafeHashMap<int, AilmentElementRegistry>(items.Length, Allocator.Persistent);
            var sharedData = new NativeHashMap<int, Entity>();
            for (var i = 0; i < items.Length; i++)
            {
                var el = items[i];

                ailments[el.id] = el;
            }

            var database = new AilmentDatabaseSingleton
            {
                database = ailments,
            };
            state.EntityManager.AddComponentData(state.SystemHandle, database);
            Debug.Log($"Ailment db was created with: {database.database.Count} ailments");
        }

        public void OnDestroy(ref SystemState state)
        {
            if (SystemAPI.HasComponent<GameItemDatabase>(state.SystemHandle))
            {
                var db = SystemAPI.GetComponent<GameItemDatabase>(state.SystemHandle);
                db.database.Dispose();
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!_initialized) Initialize(ref state);
        }

    }
}