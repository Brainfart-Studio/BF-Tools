using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Core.GlobalBootstrapper.Editor
{
    public static class BFGlobalBootstrapConfigCreator
    {
        private const string TargetPath = "Assets/Resources/BFTools";
        private const string AssetName = "GlobalBootstrapConfig.asset";

        [MenuItem("Assets/Create/BFTools/Config/Global Bootstrap Config")]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFGlobalBootstrapperConfig>(TargetPath, AssetName);
        }
    }
}