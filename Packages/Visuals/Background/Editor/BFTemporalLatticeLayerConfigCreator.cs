using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Visuals.EditorAssetUtility.Editor;

namespace BFTools.Visuals.Background.Editor
{
    public static class BFTemporalLatticeLayerConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Visuals/Background/Layers/TemporalLattice";
        private const string AssetName = "TemporalLatticeLayerConfig.asset";

        [MenuItem("Assets/Create/BFTools/Visuals/Background/Config/Layer/Temporal Lattice Layer Config", priority = BFVisualsMenuPriority.Background)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFTemporalLatticeLayerConfig>(TargetPath, AssetName);
        }
    }
}
