using UnityEditor;
using UnityEngine;

namespace BFTools.Core.GlobalBootstrapper.Editor
{
    public static class BFGlobalBootstrapConfigCreator
    {
        private const string TargetPath = "Assets/Resources/BFTools";
        private const string AssetName = "GlobalBootstrapConfig.asset";

        [MenuItem("Assets/Create/BFTools/Config/Global Bootstrap Config")]
        private static void Create()
        {
            if (!AssetDatabase.IsValidFolder(TargetPath))
                CreateFolderRecursive(TargetPath);

            string fullPath = $"{TargetPath}/{AssetName}";

            if (AssetDatabase.LoadAssetAtPath<BFGlobalBootstrapperConfig>(fullPath) != null)
            {
                Debug.LogWarning($"GlobalBootstrapConfig already exists at {fullPath}");
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<BFGlobalBootstrapperConfig>(fullPath);
                return;
            }

            BFGlobalBootstrapperConfig asset = ScriptableObject.CreateInstance<BFGlobalBootstrapperConfig>();
            AssetDatabase.CreateAsset(asset, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private static void CreateFolderRecursive(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}