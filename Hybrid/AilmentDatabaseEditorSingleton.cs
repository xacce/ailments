#if UNITY_EDITOR
using Src.PackageCandidate.Ailments.Hybrid;
using Src.PackageCandidate.EditorHelpers;
using Src.PackageCandidate.PrefabCollection.Authoring.So;
using UnityEditor;
using UnityEngine;

namespace Src.PackageCandidate.PrefabCollection.Authoring
{
    public class AilmentUpdater : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            EditorDatabasesUpdater<AilmentDatabaseEditorSingleton, AilmentBakedSo>.OnPostprocessAllAssets(AilmentDatabaseEditorSingleton.instance, importedAssets, deletedAssets,
                movedAssets, movedFromAssetPaths);
        }
    }

    [CreateAssetMenu(menuName = "Editor/Ailment database")]
    public class AilmentDatabaseEditorSingleton : EditorDatabase<AilmentDatabaseEditorSingleton, AilmentBakedSo>
    {
        [MenuItem("Tools/Build/Re-scan ailment databases")]
        public static void ForceRebuild()
        {
            instance.ForceRescan();
        }
    }
}
#endif