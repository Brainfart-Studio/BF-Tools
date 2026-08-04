using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Systems.ObjectPooler.Editor
{
    public static class BFObjectPoolerVariantCreator
    {
        private const string BasePrefabPath = "Packages/com.bftools.systems/ObjectPooler/Prefabs/ObjectPooler.prefab";
        private const string TargetPath = "Assets/Prefabs/Systems";
        private const string AssetName = "ObjectPooler.prefab";

        [MenuItem("Assets/Create/BFTools/Systems/Prefabs/Object Pooler", priority = BFMenuPriority.Group.Systems + BFMenuPriority.Module.ObjectPooler)]
        private static void Create()
        {
            BFEditorAssetUtility.CreatePrefabVariant(BasePrefabPath, TargetPath, AssetName);
        }
    }
}