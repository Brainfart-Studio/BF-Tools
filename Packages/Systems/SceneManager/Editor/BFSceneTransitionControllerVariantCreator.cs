using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Systems.SceneManager.Editor
{
    public static class BFSceneTransitionControllerVariantCreator
    {
        private const string BasePrefabPath = "Packages/com.bftools.systems/SceneManager/Prefabs/SceneTransitionController.prefab";
        private const string TargetPath = "Assets/Prefabs/Systems";
        private const string AssetName = "SceneTransitionController.prefab";

        [MenuItem("Assets/Create/BFTools/Systems/Prefabs/Scene Transition Controller", priority = BFMenuPriority.Group.Systems + BFMenuPriority.Module.SceneManager)]
        private static void Create()
        {
            BFEditorAssetUtility.CreatePrefabVariant(BasePrefabPath, TargetPath, AssetName);
        }
    }
}