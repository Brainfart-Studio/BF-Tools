using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Visuals.EditorAssetUtility.Editor;

namespace BFTools.Visuals.Background.Editor
{
    public static class BFGradientLayerConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Visuals/Background/Layers/Gradient";
        private const string AssetName = "GradientLayerConfig.asset";

        [MenuItem("Assets/Create/BFTools/Visuals/Background/Config/Layer/Gradient Layer Config", priority = BFVisualsMenuPriority.Background)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFGradientLayerConfig>(TargetPath, AssetName);
        }
    }
}