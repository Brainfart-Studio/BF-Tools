using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Visuals.EditorAssetUtility.Editor;

namespace BFTools.Visuals.Background.Editor
{
    public static class BFContradictionFieldLayerConfigCreator
    {
        private const string TargetPath =
            "Assets/Configs/Visuals/Background/Layers/ContradictionField";

        private const string AssetName =
            "ContradictionFieldLayerConfig.asset";

        [MenuItem(
            "Assets/Create/BFTools/Visuals/Background/Config/Layer/Contradiction Field Layer Config",
            priority = BFVisualsMenuPriority.Background)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFContradictionFieldLayerConfig>(
                TargetPath,
                AssetName);
        }
    }
}