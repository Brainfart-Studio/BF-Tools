using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Feedback.EditorAssetUtility.Editor;

namespace BFTools.Feedback.SpriteFlash.Editor
{
    public static class BFSpriteFlashConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Feedback/SpriteFlash";
        private const string AssetName = "SpriteFlashConfig.asset";

        [MenuItem("Assets/Create/BFTools/Feedback/Config/Sprite Flash Config", priority = BFFeedbackMenuPriority.SpriteFlash)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFSpriteFlashConfig>(TargetPath, AssetName);
        }
    }
}