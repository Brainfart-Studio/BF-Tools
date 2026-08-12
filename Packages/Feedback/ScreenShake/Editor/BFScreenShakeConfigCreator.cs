using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Feedback.EditorAssetUtility.Editor;

namespace BFTools.Feedback.ScreenShake.Editor
{
    public static class BFScreenShakeConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Feedback/ScreenShake";
        private const string AssetName = "ScreenShakeConfig.asset";

        [MenuItem("Assets/Create/BFTools/Feedback/Config/Screen Shake Config", priority = BFFeedbackMenuPriority.ScreenShake)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFScreenShakeConfig>(TargetPath, AssetName);
        }
    }
}