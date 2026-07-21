using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Core.LevelBootstrapper.Editor
{
    public static class BFLevelBootstrapConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Core/LevelBootstrapper";
        private const string AssetName = "LevelBootstrapConfig.asset";

        [MenuItem("Assets/Create/BFTools/Config/Level Bootstrap Config")]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFLevelBootstrapConfig>(TargetPath, AssetName);
        }
    }
}