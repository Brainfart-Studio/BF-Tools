using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Visuals.EditorAssetUtility.Editor;

namespace BFTools.Visuals.Background.Editor
{
    public static class BFAtmosphericFieldLayerConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Visuals/Background/Layers/AtmosphericField";
        private const string AssetName = "AtmosphericFieldLayerConfig.asset";

        [MenuItem("Assets/Create/BFTools/Visuals/Background/Config/Layer/Atmospheric Field Layer Config", priority = BFVisualsMenuPriority.Background)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFAtmosphericFieldLayerConfig>(TargetPath, AssetName);
        }
    }
}