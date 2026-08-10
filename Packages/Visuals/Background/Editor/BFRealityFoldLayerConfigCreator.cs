using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Visuals.EditorAssetUtility.Editor;

namespace BFTools.Visuals.Background.Editor
{
    public static class BFRealityFoldLayerConfigCreator
    {
        private const string TargetPath =
            "Assets/Configs/Visuals/Background/Layers/RealityFold";

        private const string AssetName =
            "RealityFoldLayerConfig.asset";

        [MenuItem(
            "Assets/Create/BFTools/Visuals/Background/Config/Layer/Reality Fold Layer Config",
            priority = BFVisualsMenuPriority.Background)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFRealityFoldLayerConfig>(
                TargetPath,
                AssetName);
        }
    }
}