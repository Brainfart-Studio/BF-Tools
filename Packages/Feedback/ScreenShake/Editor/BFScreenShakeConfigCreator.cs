using UnityEditor;
using UnityEngine;
namespace BFTools.Feedback.ScreenShake.Editor
{
    public static class BFScreenShakeConfigCreator
    {
        private const string TargetPath = "Assets/Configs/Feedback/ScreenShake";
        private const string AssetName = "ScreenShakeConfig.asset";
        [MenuItem("Assets/Create/BFTools/Config/Screen Shake Config")]
        private static void Create()
        {
            if (!AssetDatabase.IsValidFolder(TargetPath))
                CreateFolderRecursive(TargetPath);
            string fullPath = $"{TargetPath}/{AssetName}";
            if (AssetDatabase.LoadAssetAtPath<BFScreenShakeConfig>(fullPath) != null)
            {
                Debug.LogWarning($"ScreenShakeConfig already exists at {fullPath}");
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<BFScreenShakeConfig>(fullPath);
                return;
            }
            BFScreenShakeConfig asset = ScriptableObject.CreateInstance<BFScreenShakeConfig>();
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