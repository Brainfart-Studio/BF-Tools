using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Core.GlobalBootstrapper.Editor
{
    public static class BFGlobalBootstrapConfigCreator
    {
        private const string TargetPath = "Assets/Resources/BFTools";
        private const string AssetName = "GlobalBootstrapConfig.asset";

        [MenuItem("Assets/Create/BFTools/Core/Config/Global Bootstrap Config", priority = BFMenuPriority.Group.Core + BFMenuPriority.Module.GlobalBootstrapper)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFGlobalBootstrapperConfig>(TargetPath, AssetName);
        }
    }
}