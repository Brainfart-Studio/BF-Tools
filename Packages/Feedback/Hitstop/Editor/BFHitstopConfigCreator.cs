using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Feedback.EditorAssetUtility.Editor;
namespace BFTools.Feedback.Hitstop.Editor
{
    public static class BFHitstopConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Feedback/Hitstop";
        private const string AssetName = "HitstopConfig.asset";
        [MenuItem("Assets/Create/BFTools/Feedback/Config/Hitstop Config", priority = BFFeedbackMenuPriority.Hitstop)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFHitstopConfig>(TargetPath, AssetName);
        }
    }
}