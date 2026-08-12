using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Core.ProjectSetup.Editor;

namespace BFTools.Core.Logger.Editor
{
    public class BFLoggerConfigCreator : IBFProjectSetupStep
    {
        private const string TargetPath = "Assets/Resources/BFTools";
        private const string AssetName = "BFLoggerConfig.asset";

        public int Order => 0;
        public string DisplayName => "Logger Config";

        [MenuItem("Assets/Create/BFTools/Core/Config/Logger Config", priority = BFMenuPriority.Group.Core + BFMenuPriority.Module.Logger)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFLoggerConfig>(TargetPath, AssetName);
        }

        public string Run()
        {
            BFLoggerConfig config = BFEditorAssetUtility.CreateConfigAsset<BFLoggerConfig>(TargetPath, AssetName);
            return config != null ? $"{DisplayName} at {TargetPath}/{AssetName}" : null;
        }
    }
}