using UnityEditor;
using UnityEngine;
using BFTools.Core.Logger;

namespace BFTools.Core.EditorAssetUtility.Editor
{
    public static class BFEditorAssetUtility
    {
        private const string LogTag = "EditorAssetUtility";

        public static void EnsureFolderExists(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                BFLogger.Error(LogTag, "assetPath is null or empty.");
                return;
            }

            string trimmed = assetPath.Trim().TrimEnd('/');
            if (AssetDatabase.IsValidFolder(trimmed))
                return;

            string[] parts = trimmed.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string part = parts[i];
                if (string.IsNullOrEmpty(part))
                {
                    BFLogger.Error(LogTag, $"'{assetPath}' contains an empty path segment.");
                    return;
                }

                string next = $"{current}/{part}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    string guid = AssetDatabase.CreateFolder(current, part);
                    if (string.IsNullOrEmpty(guid) || !AssetDatabase.IsValidFolder(next))
                    {
                        BFLogger.Error(LogTag, $"failed to create folder '{next}'.");
                        return;
                    }

                    BFLogger.Debug(LogTag, $"Created folder '{next}'.");
                }

                current = next;
            }
        }

        public static T CreateConfigAsset<T>(string folderPath, string assetName) where T : ScriptableObject
        {
            EnsureFolderExists(folderPath);

            string fullPath = $"{folderPath}/{assetName}";
            T existing = AssetDatabase.LoadAssetAtPath<T>(fullPath);
            if (existing != null)
            {
                BFLogger.Warning(LogTag, $"{typeof(T).Name} already exists at {fullPath}");
                Selection.activeObject = existing;
                EditorGUIUtility.PingObject(existing);
                return existing;
            }

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                BFLogger.Error(LogTag, $"cannot create '{assetName}', folder '{folderPath}' does not exist.");
                return null;
            }

            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            BFLogger.Info(LogTag, $"Created {typeof(T).Name} at {fullPath}");

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            return asset;
        }

        public static GameObject CreatePrefabVariant(string basePrefabPath, string folderPath, string assetName)
        {
            GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePrefabPath);
            if (basePrefab == null)
            {
                BFLogger.Error(LogTag, $"base prefab not found at {basePrefabPath}");
                return null;
            }

            EnsureFolderExists(folderPath);

            string fullPath = $"{folderPath}/{assetName}";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
            if (existing != null)
            {
                BFLogger.Warning(LogTag, $"Prefab variant already exists at {fullPath}");
                Selection.activeObject = existing;
                EditorGUIUtility.PingObject(existing);
                return existing;
            }

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                BFLogger.Error(LogTag, $"cannot create '{assetName}', folder '{folderPath}' does not exist.");
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
            GameObject variant = PrefabUtility.SaveAsPrefabAsset(instance, fullPath);
            Object.DestroyImmediate(instance);

            BFLogger.Info(LogTag, $"Created prefab variant at {fullPath}");

            Selection.activeObject = variant;
            EditorGUIUtility.PingObject(variant);
            return variant;
        }
    }
}