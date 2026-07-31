using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Visuals.Palette.Editor
{
    public static class BFPaletteConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Visuals/Palette";
        private const string AssetName = "PaletteConfig.asset";

        [MenuItem("Assets/Create/BFTools/Visuals/Config/Palette Config", priority = BFMenuPriority.Group.Visuals + BFMenuPriority.Module.Palette)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFPaletteConfig>(TargetPath, AssetName);
        }
    }
}