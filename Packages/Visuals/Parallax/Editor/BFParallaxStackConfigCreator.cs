using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Visuals.Parallax.Editor
{
    public static class BFParallaxStackConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Visuals/Parallax";
        private const string AssetName = "ParallaxStackConfig.asset";

        [MenuItem("Assets/Create/BFTools/Visuals/Parallax/Config/Parallax Stack Config", priority = BFMenuPriority.Group.Visuals + BFMenuPriority.Module.Parallax)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFParallaxStackConfig>(TargetPath, AssetName);
        }
    }
}