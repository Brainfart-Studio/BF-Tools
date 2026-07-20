using UnityEditor;
using UnityEngine;
namespace BFTools.Feedback.ScreenShake.Editor
{
    public static class BFScreenShakeVariantCreator
    {
        private const string BasePrefabPath = "Packages/com.bftools.feedback/ScreenShake/Prefabs/ScreenShake.prefab";
        private const string TargetPath = "Assets/Prefabs/Feedback";
        private const string AssetName = "ScreenShake.prefab";
        [MenuItem("Assets/Create/BFTools/Prefabs/Screen Shake")]
        private static void Create()
        {
            GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BasePrefabPath);
            if (basePrefab == null)
            {
                Debug.LogError($"BFScreenShakeVariantCreator: Base prefab not found at {BasePrefabPath}");
                return;
            }
            if (!AssetDatabase.IsValidFolder(TargetPath))
                CreateFolderRecursive(TargetPath);
            string fullPath = $"{TargetPath}/{AssetName}";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(fullPath) != null)
            {
                Debug.LogWarning($"Prefab variant already exists at {fullPath}");
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
                return;
            }
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
            GameObject variant = PrefabUtility.SaveAsPrefabAsset(instance, fullPath);
            Object.DestroyImmediate(instance);
            Selection.activeObject = variant;
            EditorGUIUtility.PingObject(variant);
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