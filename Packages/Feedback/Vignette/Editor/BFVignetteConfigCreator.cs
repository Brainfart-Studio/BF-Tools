using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Feedback.EditorAssetUtility.Editor;

namespace BFTools.Feedback.Vignette.Editor
{
    public static class BFVignetteConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Feedback/Vignette";
        private const string AssetName = "VignetteConfig.asset";

        [MenuItem("Assets/Create/BFTools/Feedback/Config/Vignette Config", priority = BFFeedbackMenuPriority.Vignette)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFVignetteConfig>(TargetPath, AssetName);
        }
    }
}