using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Feedback.EditorAssetUtility.Editor;

namespace BFTools.Feedback.ScreenFlash.Editor
{
    public static class BFScreenFlashConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Feedback/ScreenFlash";
        private const string AssetName = "ScreenFlashConfig.asset";

        [MenuItem("Assets/Create/BFTools/Feedback/Config/Screen Flash Config", priority = BFFeedbackMenuPriority.ScreenFlash)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFScreenFlashConfig>(TargetPath, AssetName);
        }
    }
}