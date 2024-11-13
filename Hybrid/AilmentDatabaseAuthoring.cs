// using GameReady.Ailments.Runtime;
// using Src.PackageCandidate.GameReady.Ailments.Hybrid;
// using Unity.Entities;
// using UnityEngine;
//
// namespace GameReady.Ailments.Hybrid
// {
//     public class AilmentDatabaseAuthoring : MonoBehaviour
//     {
//         private class AilmentDatabaseBaker : Baker<AilmentDatabaseAuthoring>
//         {
//             public override void Bake(AilmentDatabaseAuthoring authoring)
//             {
//                 var e = GetEntity(TransformUsageFlags.None);
//                 var ailments = AddBuffer<AilmentDatabaseElement>(e);
//                 foreach (var ailment in Core.Hybrid.Helpers.FindAllAssetsByType<SetTagAilment>())
//                 {
//                     ailments.Add(
//                         new AilmentDatabaseElement()
//                         {
//                             blob = ailment.Bake(this),
//                             ailmentId = ailment.id,
//                         });
//                 }
//             }
//         }
//     }
// }