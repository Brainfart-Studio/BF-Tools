using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Visuals.EditorAssetUtility.Editor;

namespace BFTools.Visuals.Background.Editor
{
    public static class BFImpossibleGardenLayerConfigCreator
    {
        private const string TargetPath =
            "Assets/Configs/Visuals/Background/Layers/ImpossibleGarden";

        private const string AssetName =
            "ImpossibleGardenLayerConfig.asset";

        [MenuItem(
            "Assets/Create/BFTools/Visuals/Background/Config/Layer/Impossible Garden Layer",
            priority = BFVisualsMenuPriority.Background)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFImpossibleGardenLayerConfig>(
                TargetPath,
                AssetName);
        }
    }
}