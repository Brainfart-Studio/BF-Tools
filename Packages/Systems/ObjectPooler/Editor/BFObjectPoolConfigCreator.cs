using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Systems.EditorAssetUtility.Editor;

namespace BFTools.Systems.ObjectPooler.Editor
{
    public static class BFObjectPoolConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Systems/ObjectPooler";
        private const string AssetName = "ObjectPoolConfig.asset";

        [MenuItem("Assets/Create/BFTools/Systems/Config/Object Pool Config", priority = BFSystemsMenuPriority.ObjectPooler)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFObjectPoolConfig>(TargetPath, AssetName);
        }
    }
}