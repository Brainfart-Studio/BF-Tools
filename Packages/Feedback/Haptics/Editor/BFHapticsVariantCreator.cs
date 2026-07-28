using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Feedback.Haptics.Editor
{
    public static class BFHapticsVariantCreator
    {
        private const string BasePrefabPath = "Packages/com.bftools.feedback/Haptics/Prefabs/Haptics.prefab";
        private const string TargetPath = "Assets/Prefabs/Feedback";
        private const string AssetName = "Haptics.prefab";

        [MenuItem("Assets/Create/BFTools/Feedback/Prefabs/Haptics", priority = BFMenuPriority.Group.Feedback + BFMenuPriority.Module.Haptics)]
        private static void Create()
        {
            BFEditorAssetUtility.CreatePrefabVariant(BasePrefabPath, TargetPath, AssetName);
        }
    }
}