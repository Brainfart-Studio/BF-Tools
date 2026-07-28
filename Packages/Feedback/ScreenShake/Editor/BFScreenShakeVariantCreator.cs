using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Feedback.ScreenShake.Editor
{
    public static class BFScreenShakeVariantCreator
    {
        private const string BasePrefabPath = "Packages/com.bftools.feedback/ScreenShake/Prefabs/ScreenShake.prefab";
        private const string TargetPath = "Assets/Prefabs/Feedback";
        private const string AssetName = "ScreenShake.prefab";

        [MenuItem("Assets/Create/BFTools/Feedback/Prefabs/Screen Shake", priority = BFMenuPriority.Group.Feedback + BFMenuPriority.Module.ScreenShake)]
        private static void Create()
        {
            BFEditorAssetUtility.CreatePrefabVariant(BasePrefabPath, TargetPath, AssetName);
        }
    }
}