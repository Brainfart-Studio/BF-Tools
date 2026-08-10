using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Visuals.EditorAssetUtility.Editor;

namespace BFTools.Visuals.Background.Editor
{
    public static class BFImpossibleWeaveLayerConfigCreator
    {
        private const string TargetPath =
            "Assets/Configs/Visuals/Background/Layers/ImpossibleWeave";

        private const string AssetName =
            "ImpossibleWeaveLayerConfig.asset";

        [MenuItem(
            "Assets/Create/BFTools/Visuals/Background/Config/Layer/Impossible Weave Layer Config",
            priority = BFVisualsMenuPriority.Background)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFImpossibleWeaveLayerConfig>(
                TargetPath,
                AssetName);
        }
    }
}