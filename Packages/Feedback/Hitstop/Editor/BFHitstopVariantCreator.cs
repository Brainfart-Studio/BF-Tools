using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Feedback.EditorAssetUtility.Editor;
namespace BFTools.Feedback.Hitstop.Editor
{
    public static class BFHitstopVariantCreator
    {
        private const string BasePrefabPath = "Packages/com.bftools.feedback/Hitstop/Prefabs/Hitstop.prefab";
        private const string TargetPath = "Assets/Prefabs/Feedback";
        private const string AssetName = "Hitstop.prefab";
        [MenuItem("Assets/Create/BFTools/Feedback/Prefabs/Hitstop", priority = BFFeedbackMenuPriority.Hitstop)]
        private static void Create()
        {
            BFEditorAssetUtility.CreatePrefabVariant(BasePrefabPath, TargetPath, AssetName);
        }
    }
}