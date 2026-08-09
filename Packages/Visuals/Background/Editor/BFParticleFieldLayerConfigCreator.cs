using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Visuals.EditorAssetUtility.Editor;

namespace BFTools.Visuals.Background.Editor
{
    public static class BFParticleFieldLayerConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Visuals/Background/Layers/ParticleField";
        private const string AssetName = "ParticleFieldLayerConfig.asset";

        [MenuItem("Assets/Create/BFTools/Visuals/Background/Config/Layer/Particle Field Layer Config", priority = BFVisualsMenuPriority.Background)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFParticleFieldLayerConfig>(TargetPath, AssetName);
        }
    }
}