using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Visuals.EditorAssetUtility.Editor;

namespace BFTools.Visuals.Parallax.Editor
{
    public static class BFParallaxStackManagerVariantCreator
    {
        private const string BasePrefabPath = "Packages/com.bftools.visuals/Parallax/Prefabs/ParallaxStackManager.prefab";
        private const string TargetPath = "Assets/Prefabs/Visuals/Parallax";
        private const string AssetName = "ParallaxStackManager.prefab";

        [MenuItem("Assets/Create/BFTools/Visuals/Parallax/Prefabs/Parallax Stack Manager", priority = BFVisualsMenuPriority.Parallax)]
        private static void Create()
        {
            BFEditorAssetUtility.CreatePrefabVariant(BasePrefabPath, TargetPath, AssetName);
        }
    }
}