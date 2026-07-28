using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Core.Logger.Editor
{
    public static class BFLoggerConfigCreator
    {
        private const string TargetPath = "Assets/Resources/BFTools";
        private const string AssetName = "BFLoggerConfig.asset";

        [MenuItem("Assets/Create/BFTools/Core/Config/Logger Config", priority = BFMenuPriority.Group.Core + BFMenuPriority.Module.Logger)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFLoggerConfig>(TargetPath, AssetName);
        }
    }
}