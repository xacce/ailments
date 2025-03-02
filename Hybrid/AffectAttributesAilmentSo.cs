#if UNITY_EDITOR
using GameReady.Ailments.Runtime;
using Src.PackageCandidate.Ailments.Runtime;
using Src.PackageCandidate.Attributer;
using Unity.Entities;
using UnityEngine;

namespace Src.PackageCandidate.Ailments.Hybrid
{
    [CreateAssetMenu(menuName = "Sufferenger/Ailments/Affect attrs ailment")]
    public class AffectAttributesAilmentSo : AilmentBakedSo
    {
        [SerializeField] private AttributeValuesPresetSo affectAttributes;
        [SerializeField] private AilmentBlobRootBaked root;

        public override int id => root.stackGroupId;
        public override Ailment ailment => new AffectAttributesAilment { };

        public override void Bake(ref AilmentBlob data, ref BlobBuilder blobBuilder)
        {
            affectAttributes.BakeToShort(ref blobBuilder, ref data.polyData.attributes);
            root.Bake(ref blobBuilder,ref data.root);
        }
    }
}
#endif