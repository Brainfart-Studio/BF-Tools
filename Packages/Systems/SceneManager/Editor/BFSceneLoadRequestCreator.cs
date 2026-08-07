using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Systems.EditorAssetUtility.Editor;

namespace BFTools.Systems.SceneManager.Editor
{
    public static class BFSceneLoadRequestCreator
    {
        private const string TargetPath = "Assets/Configs/Systems/SceneManager";
        private const string AssetName = "SceneLoadRequest.asset";

        [MenuItem("Assets/Create/BFTools/Systems/Config/Scene Load Request", priority = BFSystemsMenuPriority.SceneManager)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFSceneLoadRequest>(TargetPath, AssetName);
        }
    }
}