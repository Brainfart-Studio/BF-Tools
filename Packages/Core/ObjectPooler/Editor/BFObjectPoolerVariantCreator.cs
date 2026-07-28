using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Core.ObjectPooler.Editor
{
    public static class BFObjectPoolerVariantCreator
    {
        private const string BasePrefabPath = "Packages/com.bftools.core/ObjectPooler/Prefabs/ObjectPooler.prefab";
        private const string TargetPath = "Assets/Prefabs/Core";
        private const string AssetName = "ObjectPooler.prefab";

        [MenuItem("Assets/Create/BFTools/Core/Prefabs/Object Pooler", priority = BFMenuPriority.Group.Core + BFMenuPriority.Module.ObjectPooler)]
        private static void Create()
        {
            BFEditorAssetUtility.CreatePrefabVariant(BasePrefabPath, TargetPath, AssetName);
        }
    }
}