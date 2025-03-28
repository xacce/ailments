#if UNITY_EDITOR
using GameReady.Ailments.Runtime;
using Src.PackageCandidate.Ailments.Runtime;
using Unity.Entities;
using UnityEngine;

namespace Src.PackageCandidate.Ailments.Hybrid
{
    [CreateAssetMenu(menuName = "Sufferenger/Ailments/Set tag ailment")]
    public class AddTagAilmentSo : AilmentBakedSo
    {
        [SerializeField] private AilmentTag tag;
        [SerializeField] private AilmentBlobRootBaked root;

        public override int id => root.stackGroupId;
        public override Ailment ailment => new AddTagAilment { };

        public override void Bake(ref AilmentBlob data, ref BlobBuilder blobBuilder)
        {
            data.polyData.i1 = (int)tag;
            data.polyData.value = 0;
            root.Bake(ref blobBuilder,ref data.root);
        }
    }
}
#endif