using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Visuals.EditorAssetUtility.Editor;

namespace BFTools.Visuals.Background.Editor
{
    public static class BFLightningFieldLayerConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Visuals/Background/Layers/LightningField";
        private const string AssetName = "LightningFieldLayerConfig.asset";

        [MenuItem("Assets/Create/BFTools/Visuals/Background/Config/Layer/Lightning Field Layer Config", priority = BFVisualsMenuPriority.Background)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFLightningFieldLayerConfig>(TargetPath, AssetName);
        }
    }
}