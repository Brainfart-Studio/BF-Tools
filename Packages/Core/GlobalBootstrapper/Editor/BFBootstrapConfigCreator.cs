using UnityEditor;
using UnityEngine;

namespace BFTools.Core.Bootstrapper.Editor
{
    public static class BFBootstrapConfigCreator
    {
        private const string TargetPath = "Assets/Resources/BFTools";
        private const string AssetName = "BootstrapConfig.asset";

        [MenuItem("Assets/Create/BFTools/Config/Bootstrap Config")]
        private static void Create()
        {
            if (!AssetDatabase.IsValidFolder(TargetPath))
                CreateFolderRecursive(TargetPath);

            string fullPath = $"{TargetPath}/{AssetName}";

            if (AssetDatabase.LoadAssetAtPath<BootstrapConfig>(fullPath) != null)
            {
                Debug.LogWarning($"BootstrapConfig already exists at {fullPath}");
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<BootstrapConfig>(fullPath);
                return;
            }

            BootstrapConfig asset = ScriptableObject.CreateInstance<BootstrapConfig>();
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