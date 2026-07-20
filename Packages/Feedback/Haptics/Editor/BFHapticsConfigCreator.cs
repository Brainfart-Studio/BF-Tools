using UnityEditor;
using UnityEngine;

namespace BFTools.Feedback.Haptics.Editor
{
    public static class BFHapticsConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Feedback/Haptics";
        private const string AssetName = "HapticsConfig.asset";

        [MenuItem("Assets/Create/BFTools/Config/Haptics Config")]
        private static void Create()
        {
            if (!AssetDatabase.IsValidFolder(TargetPath))
                CreateFolderRecursive(TargetPath);

            string fullPath = $"{TargetPath}/{AssetName}";
            if (AssetDatabase.LoadAssetAtPath<BFHapticsConfig>(fullPath) != null)
            {
                Debug.LogWarning($"HapticsConfig already exists at {fullPath}");
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<BFHapticsConfig>(fullPath);
                return;
            }

            BFHapticsConfig asset = ScriptableObject.CreateInstance<BFHapticsConfig>();
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