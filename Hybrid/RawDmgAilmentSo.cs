#if UNITY_EDITOR
using GameReady.Ailments.Runtime;
using Src.PackageCandidate.Ailments.Hybrid;
using Src.PackageCandidate.Ailments.Runtime;
using Src.PackageCandidate.Sufferenger.Authoring;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Src.PackageCandidate.GameReady.Ailments.Hybrid
{
    [CreateAssetMenu(menuName = "Sufferenger/Ailments/Raw dmg ailment")]
    public class RawDmgAilmentSo : AilmentBakedSo
    {
        [SerializeField] private AilmentBlobRootBaked root;
        [SerializeField] private int2 dmgIndex;
        [SerializeField] private float baseValue;
        public override int id => root.stackGroupId;
        public override Ailment ailment => new RawDmgAilment { };
        public override void Bake(ref AilmentBlob data, ref BlobBuilder blobBuilder)
        {
            data.polyData.int2 = dmgIndex;
            data.polyData.f1 = baseValue;
            root.Bake(ref blobBuilder, ref data.root);
        }
    }
}
#endif