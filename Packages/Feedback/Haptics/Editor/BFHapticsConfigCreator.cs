using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Feedback.Haptics.Editor
{
    public static class BFHapticsConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Feedback/Haptics";
        private const string AssetName = "HapticsConfig.asset";

        [MenuItem("Assets/Create/BFTools/Config/Haptics Config")]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFHapticsConfig>(TargetPath, AssetName);
        }
    }
}