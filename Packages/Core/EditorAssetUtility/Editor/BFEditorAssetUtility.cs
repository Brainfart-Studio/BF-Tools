using UnityEditor;
using UnityEngine;

namespace BFTools.Core.EditorAssetUtility.Editor
{
    public static class BFEditorAssetUtility
    {
        public static void EnsureFolderExists(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError("BFEditorAssetUtility: assetPath is null or empty.");
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
                    Debug.LogError($"BFEditorAssetUtility: '{assetPath}' contains an empty path segment.");
                    return;
                }

                string next = $"{current}/{part}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    string guid = AssetDatabase.CreateFolder(current, part);
                    if (string.IsNullOrEmpty(guid) || !AssetDatabase.IsValidFolder(next))
                    {
                        Debug.LogError($"BFEditorAssetUtility: failed to create folder '{next}'.");
                        return;
                    }
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
                Debug.LogWarning($"{typeof(T).Name} already exists at {fullPath}");
                Selection.activeObject = existing;
                EditorGUIUtility.PingObject(existing);
                return existing;
            }

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"BFEditorAssetUtility: cannot create '{assetName}', folder '{folderPath}' does not exist.");
                return null;
            }

            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            return asset;
        }

        public static GameObject CreatePrefabVariant(string basePrefabPath, string folderPath, string assetName)
        {
            GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePrefabPath);
            if (basePrefab == null)
            {
                Debug.LogError($"BFEditorAssetUtility: base prefab not found at {basePrefabPath}");
                return null;
            }

            EnsureFolderExists(folderPath);

            string fullPath = $"{folderPath}/{assetName}";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
            if (existing != null)
            {
                Debug.LogWarning($"Prefab variant already exists at {fullPath}");
                Selection.activeObject = existing;
                EditorGUIUtility.PingObject(existing);
                return existing;
            }

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"BFEditorAssetUtility: cannot create '{assetName}', folder '{folderPath}' does not exist.");
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
            GameObject variant = PrefabUtility.SaveAsPrefabAsset(instance, fullPath);
            Object.DestroyImmediate(instance);

            Selection.activeObject = variant;
            EditorGUIUtility.PingObject(variant);
            return variant;
        }
    }
}