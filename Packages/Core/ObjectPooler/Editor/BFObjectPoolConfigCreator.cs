using UnityEditor;
using BFTools.Core.EditorAssetUtility.Editor;

namespace BFTools.Core.ObjectPooler.Editor
{
    public static class BFObjectPoolConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Core/ObjectPooler";
        private const string AssetName = "ObjectPoolConfig.asset";

        [MenuItem("Assets/Create/BFTools/Core/Config/Object Pool Config", priority = -100)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFObjectPoolConfig>(TargetPath, AssetName);
        }
    }
}