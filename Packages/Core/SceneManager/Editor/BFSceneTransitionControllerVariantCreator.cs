using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Core.SceneManager.Editor
{
    public static class BFSceneTransitionControllerVariantCreator
    {
        private const string BasePrefabPath = "Packages/com.bftools.core/SceneManager/Prefabs/SceneTransitionController.prefab";
        private const string TargetPath = "Assets/Prefabs/Core";
        private const string AssetName = "SceneTransitionController.prefab";

        [MenuItem("Assets/Create/BFTools/Core/Prefabs/Scene Transition Controller", priority = BFMenuPriority.Group.Core + BFMenuPriority.Module.SceneManager)]
        private static void Create()
        {
            BFEditorAssetUtility.CreatePrefabVariant(BasePrefabPath, TargetPath, AssetName);
        }
    }
}