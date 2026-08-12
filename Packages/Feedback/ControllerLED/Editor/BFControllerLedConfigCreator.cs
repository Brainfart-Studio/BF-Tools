using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Feedback.EditorAssetUtility.Editor;

namespace BFTools.Feedback.ControllerLED.Editor
{
    public static class BFControllerLedConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Feedback/ControllerLED";
        private const string AssetName = "ControllerLedConfig.asset";

        [MenuItem("Assets/Create/BFTools/Feedback/Config/Controller LED Config", priority = BFFeedbackMenuPriority.ControllerLED)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFControllerLedConfig>(TargetPath, AssetName);
        }
    }
}