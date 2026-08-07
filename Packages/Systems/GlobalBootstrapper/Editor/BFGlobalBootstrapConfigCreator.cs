using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Systems.EditorAssetUtility.Editor;

namespace BFTools.Systems.GlobalBootstrapper.Editor
{
    public static class BFGlobalBootstrapConfigCreator
    {
        private const string TargetPath = "Assets/Resources/BFTools";
        private const string AssetName = "GlobalBootstrapConfig.asset";

        [MenuItem("Assets/Create/BFTools/Systems/Config/Global Bootstrap Config", priority = BFSystemsMenuPriority.GlobalBootstrapper)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFGlobalBootstrapperConfig>(TargetPath, AssetName);
        }
    }
}