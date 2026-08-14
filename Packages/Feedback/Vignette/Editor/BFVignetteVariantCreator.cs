using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Feedback.EditorAssetUtility.Editor;

namespace BFTools.Feedback.Vignette.Editor
{
    public static class BFVignetteVariantCreator
    {
        private const string BasePrefabPath = "Packages/com.bftools.feedback/Vignette/Prefabs/Vignette.prefab";
        private const string TargetPath = "Assets/Prefabs/Feedback";
        private const string AssetName = "Vignette.prefab";

        [MenuItem("Assets/Create/BFTools/Feedback/Prefabs/Vignette", priority = BFFeedbackMenuPriority.Vignette)]
        private static void Create()
        {
            BFEditorAssetUtility.CreatePrefabVariant(BasePrefabPath, TargetPath, AssetName);
        }
    }
}