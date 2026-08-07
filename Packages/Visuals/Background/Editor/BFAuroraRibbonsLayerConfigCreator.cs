using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Visuals.EditorAssetUtility.Editor;

namespace BFTools.Visuals.Background.Editor
{
    public static class BFAuroraRibbonsLayerConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Visuals/Background/Layers/AuroraRibbons";
        private const string AssetName = "AuroraRibbonsLayerConfig.asset";

        [MenuItem("Assets/Create/BFTools/Visuals/Background/Config/Layer/Aurora Ribbons Layer Config", priority = BFVisualsMenuPriority.Background)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFAuroraRibbonsLayerConfig>(TargetPath, AssetName);
        }
    }
}