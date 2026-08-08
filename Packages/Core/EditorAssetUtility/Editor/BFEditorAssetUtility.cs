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
            {
                BFLogger.Trace(LogTag, $"Folder '{trimmed}' already exists.");
                return;
            }

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
                else
                {
                    BFLogger.Trace(LogTag, $"Folder segment '{next}' already exists.");
                }

                current = next;
            }
        }

        public static T CreateConfigAsset<T>(string folderPath, string assetName) where T : ScriptableObject
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                BFLogger.Error(LogTag, "folderPath is null or empty.");
                return null;
            }

            if (string.IsNullOrEmpty(assetName))
            {
                BFLogger.Error(LogTag, "assetName is null or empty.");
                return null;
            }

            EnsureFolderExists(folderPath);

            string fullPath = $"{folderPath}/{assetName}";
            BFLogger.Trace(LogTag, $"Checking for existing {typeof(T).Name} at '{fullPath}'.");
            T existing = AssetDatabase.LoadAssetAtPath<T>(fullPath);
            if (TryHandleExisting(existing, fullPath, typeof(T).Name))
                return existing;

            if (!RequireValidFolder(folderPath, assetName))
                return null;

            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            BFLogger.Info(LogTag, $"Created {typeof(T).Name} at {fullPath}");

            SelectAndPing(asset);
            return asset;
        }

        public static GameObject CreatePrefabVariant(string basePrefabPath, string folderPath, string assetName)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                BFLogger.Error(LogTag, "folderPath is null or empty.");
                return null;
            }

            if (string.IsNullOrEmpty(assetName))
            {
                BFLogger.Error(LogTag, "assetName is null or empty.");
                return null;
            }

            BFLogger.Trace(LogTag, $"Loading base prefab at '{basePrefabPath}'.");
            GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePrefabPath);
            if (basePrefab == null)
            {
                BFLogger.Error(LogTag, $"base prefab not found at {basePrefabPath}");
                return null;
            }

            EnsureFolderExists(folderPath);

            string fullPath = $"{folderPath}/{assetName}";
            BFLogger.Trace(LogTag, $"Checking for existing prefab variant at '{fullPath}'.");
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
            if (TryHandleExisting(existing, fullPath, "Prefab variant"))
                return existing;

            if (!RequireValidFolder(folderPath, assetName))
                return null;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
            GameObject variant = PrefabUtility.SaveAsPrefabAsset(instance, fullPath);
            Object.DestroyImmediate(instance);

            BFLogger.Info(LogTag, $"Created prefab variant at {fullPath}");

            SelectAndPing(variant);
            return variant;
        }

        private static bool TryHandleExisting(Object existing, string fullPath, string label)
        {
            if (existing == null)
                return false;

            BFLogger.Warning(LogTag, $"{label} already exists at {fullPath}");
            SelectAndPing(existing);
            return true;
        }

        private static bool RequireValidFolder(string folderPath, string assetName)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
                return true;

            BFLogger.Error(LogTag, $"cannot create '{assetName}', folder '{folderPath}' does not exist.");
            return false;
        }

        private static void SelectAndPing(Object asset)
        {
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}