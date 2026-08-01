using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Visuals.Background.Editor
{
    public static class BFGradientLayerConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Visuals/Background/Layers/Gradient";
        private const string AssetName = "GradientLayerConfig.asset";

        [MenuItem("Assets/Create/BFTools/Visuals/Background/Config/Layer/Gradient Layer Config", priority = BFMenuPriority.Group.Visuals + BFMenuPriority.Module.Background)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFGradientLayerConfig>(TargetPath, AssetName);
        }
    }
}