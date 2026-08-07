using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Visuals.EditorAssetUtility.Editor;

namespace BFTools.Visuals.Background.Editor
{
    public static class BFTwinklingStarsLayerConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Visuals/Background/Layers/TwinklingStars";
        private const string AssetName = "TwinklingStarsLayerConfig.asset";

        [MenuItem("Assets/Create/BFTools/Visuals/Background/Config/Layer/Twinkling Stars Layer Config", priority = BFVisualsMenuPriority.Background)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFTwinklingStarsLayerConfig>(TargetPath, AssetName);
        }
    }
}