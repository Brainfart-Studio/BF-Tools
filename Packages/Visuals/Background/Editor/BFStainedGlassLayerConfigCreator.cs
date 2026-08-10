using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Visuals.EditorAssetUtility.Editor;

namespace BFTools.Visuals.Background.Editor
{
    public static class BFStainedGlassLayerConfigCreator
    {
        private const string TargetPath =
            "Assets/Configs/Visuals/Background/Layers/StainedGlass";

        private const string AssetName =
            "StainedGlassLayerConfig.asset";

        [MenuItem(
            "Assets/Create/BFTools/Visuals/Background/Config/Layer/Stained Glass Layer",
            priority = BFVisualsMenuPriority.Background)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFStainedGlassLayerConfig>(
                TargetPath,
                AssetName);
        }
    }
}