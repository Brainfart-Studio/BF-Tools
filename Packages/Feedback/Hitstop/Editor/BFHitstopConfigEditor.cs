using UnityEditor;
using UnityEngine;
using System.IO;

namespace BFTools.Feedback.Hitstop.Editor
{
    public static class BFHitstopConfigEditor
    {
        private const string TargetFolder = "Assets/BFTools/Feedback/Hitstop";

        [MenuItem("BFTools/Feedback/Hitstop/Create Hitstop Config")]
        public static void CreateConfig()
        {
            if (!Directory.Exists(TargetFolder))
                Directory.CreateDirectory(TargetFolder);

            var asset = ScriptableObject.CreateInstance<BFHitstopConfig>();
            string path = AssetDatabase.GenerateUniqueAssetPath($"{TargetFolder}/BFHitstopConfig.asset");

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}