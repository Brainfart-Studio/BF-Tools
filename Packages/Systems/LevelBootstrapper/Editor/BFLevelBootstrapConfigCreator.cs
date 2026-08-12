using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Systems.EditorAssetUtility.Editor;

namespace BFTools.Systems.LevelBootstrapper.Editor
{
    public static class BFLevelBootstrapConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Systems/LevelBootstrapper";
        private const string AssetName = "LevelBootstrapConfig.asset";

        [MenuItem("Assets/Create/BFTools/Systems/Config/Level Bootstrap Config", priority = BFSystemsMenuPriority.LevelBootstrapper)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFLevelBootstrapConfig>(TargetPath, AssetName);
        }
    }
}