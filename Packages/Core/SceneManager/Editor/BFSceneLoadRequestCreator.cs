using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Core.SceneManager.Editor
{
    public static class BFSceneLoadRequestCreator
    {
        private const string TargetPath = "Assets/Configs/Core/SceneManager";
        private const string AssetName = "SceneLoadRequest.asset";

        [MenuItem("Assets/Create/BFTools/Core/Config/Scene Load Request", priority = BFMenuPriority.Group.Core + BFMenuPriority.Module.SceneManager)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFSceneLoadRequest>(TargetPath, AssetName);
        }
    }
}