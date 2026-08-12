using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Visuals.EditorAssetUtility.Editor;

namespace BFTools.Visuals.Parallax.Editor
{
    public static class BFParallaxSpriteLayerConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Visuals/Parallax/Layers/Sprite";
        private const string AssetName = "ParallaxSpriteLayerConfig.asset";

        [MenuItem("Assets/Create/BFTools/Visuals/Parallax/Config/Layer/Sprite Layer Config", priority = BFVisualsMenuPriority.Parallax)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFParallaxSpriteLayerConfig>(TargetPath, AssetName);
        }
    }
}