using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Visuals.Background.Editor
{
    public static class BFBackgroundStackManagerVariantCreator
    {
        private const string BasePrefabPath = "Packages/com.bftools.visuals/Background/Prefabs/BackgroundStackManager.prefab";
        private const string TargetPath = "Assets/Prefabs/Visuals/Background";
        private const string AssetName = "BackgroundStackManager.prefab";

        [MenuItem("Assets/Create/BFTools/Visuals/Background/Prefabs/Background Stack Manager", priority = BFMenuPriority.Group.Visuals + BFMenuPriority.Module.Background)]
        private static void Create()
        {
            BFEditorAssetUtility.CreatePrefabVariant(BasePrefabPath, TargetPath, AssetName);
        }
    }
}