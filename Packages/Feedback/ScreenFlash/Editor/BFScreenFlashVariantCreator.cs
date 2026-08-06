using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Feedback.EditorAssetUtility.Editor;

namespace BFTools.Feedback.ScreenFlash.Editor
{
    public static class BFScreenFlashVariantCreator
    {
        private const string BasePrefabPath = "Packages/com.bftools.feedback/ScreenFlash/Prefabs/ScreenFlash.prefab";
        private const string TargetPath = "Assets/Prefabs/Feedback";
        private const string AssetName = "ScreenFlash.prefab";

        [MenuItem("Assets/Create/BFTools/Feedback/Prefabs/Screen Flash", priority = BFFeedbackMenuPriority.ScreenFlash)]
        private static void Create()
        {
            BFEditorAssetUtility.CreatePrefabVariant(BasePrefabPath, TargetPath, AssetName);
        }
    }
}