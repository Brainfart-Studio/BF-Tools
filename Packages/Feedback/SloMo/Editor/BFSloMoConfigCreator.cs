using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Feedback.EditorAssetUtility.Editor;
namespace BFTools.Feedback.SloMo.Editor
{
    public static class BFSloMoConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Feedback/SloMo";
        private const string AssetName = "SloMoConfig.asset";
        [MenuItem("Assets/Create/BFTools/Feedback/Config/SloMo Config", priority = BFFeedbackMenuPriority.SloMo)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFSloMoConfig>(TargetPath, AssetName);
        }
    }
}