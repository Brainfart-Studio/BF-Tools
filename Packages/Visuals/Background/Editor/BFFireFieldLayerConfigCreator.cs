using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Visuals.EditorAssetUtility.Editor;

namespace BFTools.Visuals.Background.Editor
{
    public static class BFFireFieldLayerConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Visuals/Background/Layers/FireField";
        private const string AssetName = "FireFieldLayerConfig.asset";

        [MenuItem("Assets/Create/BFTools/Visuals/Background/Config/Layer/Fire Field Layer Config", priority = BFVisualsMenuPriority.Background)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFFireFieldLayerConfig>(TargetPath, AssetName);
        }
    }
}