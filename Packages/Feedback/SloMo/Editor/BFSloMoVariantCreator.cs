using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Feedback.EditorAssetUtility.Editor;
namespace BFTools.Feedback.SloMo.Editor
{
    public static class BFSloMoVariantCreator
    {
        private const string BasePrefabPath = "Packages/com.bftools.feedback/SloMo/Prefabs/SloMo.prefab";
        private const string TargetPath = "Assets/Prefabs/Feedback";
        private const string AssetName = "SloMo.prefab";
        [MenuItem("Assets/Create/BFTools/Feedback/Prefabs/SloMo", priority = BFFeedbackMenuPriority.SloMo)]
        private static void Create()
        {
            BFEditorAssetUtility.CreatePrefabVariant(BasePrefabPath, TargetPath, AssetName);
        }
    }
}