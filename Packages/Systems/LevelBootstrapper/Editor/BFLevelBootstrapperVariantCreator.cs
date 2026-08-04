using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Systems.LevelBootstrapper.Editor
{
    public static class BFLevelBootstrapperVariantCreator
    {
        private const string BasePrefabPath = "Packages/com.bftools.systems/LevelBootstrapper/Prefabs/LevelBootstrapper.prefab";
        private const string TargetPath = "Assets/Prefabs/Systems";
        private const string AssetName = "LevelBootstrapper.prefab";

        [MenuItem("Assets/Create/BFTools/Systems/Prefabs/Level Bootstrapper", priority = BFMenuPriority.Group.Systems + BFMenuPriority.Module.LevelBootstrapper)]
        private static void Create()
        {
            BFEditorAssetUtility.CreatePrefabVariant(BasePrefabPath, TargetPath, AssetName);
        }
    }
}