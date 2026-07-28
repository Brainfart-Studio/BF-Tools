using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Feedback.ScreenShake.Editor
{
    public static class BFScreenShakeConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Feedback/ScreenShake";
        private const string AssetName = "ScreenShakeConfig.asset";

        [MenuItem("Assets/Create/BFTools/Feedback/Config/Screen Shake Config", priority = BFMenuPriority.Group.Feedback + BFMenuPriority.Module.ScreenShake)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFScreenShakeConfig>(TargetPath, AssetName);
        }
    }
}