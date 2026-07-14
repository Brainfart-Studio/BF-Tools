using UnityEditor;
using UnityEngine;
namespace BFTools.Core.LevelBootstrapper.Editor
{
    public static class BFLevelBootstrapConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Core/LevelBootstrapper";
        private const string AssetName = "LevelBootstrapConfig.asset";
        [MenuItem("Assets/Create/BFTools/Config/Level Bootstrap Config")]
        private static void Create()
        {
            if (!AssetDatabase.IsValidFolder(TargetPath))
                CreateFolderRecursive(TargetPath);
            string fullPath = $"{TargetPath}/{AssetName}";
            if (AssetDatabase.LoadAssetAtPath<BFLevelBootstrapConfig>(fullPath) != null)
            {
                Debug.LogWarning($"LevelBootstrapConfig already exists at {fullPath}");
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<BFLevelBootstrapConfig>(fullPath);
                return;
            }
            BFLevelBootstrapConfig asset = ScriptableObject.CreateInstance<BFLevelBootstrapConfig>();
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