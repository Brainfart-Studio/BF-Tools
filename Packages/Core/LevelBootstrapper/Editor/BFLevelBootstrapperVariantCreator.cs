using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Core.LevelBootstrapper.Editor
{
    public static class BFLevelBootstrapperVariantCreator
    {
        private const string BasePrefabPath = "Packages/com.bftools.core/LevelBootstrapper/Prefabs/LevelBootstrapper.prefab";
        private const string TargetPath = "Assets/Prefabs/Core";
        private const string AssetName = "LevelBootstrapper.prefab";

        [MenuItem("Assets/Create/BFTools/Core/Prefabs/Level Bootstrapper", priority = -100)]
        private static void Create()
        {
            BFEditorAssetUtility.CreatePrefabVariant(BasePrefabPath, TargetPath, AssetName);
        }
    }
}