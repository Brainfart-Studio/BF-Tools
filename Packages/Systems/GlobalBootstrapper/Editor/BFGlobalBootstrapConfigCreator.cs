using UnityEditor;
using UnityEngine;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Core.Logger;
using BFTools.Core.ProjectSetup.Editor;
using BFTools.Systems.EditorAssetUtility.Editor;

namespace BFTools.Systems.GlobalBootstrapper.Editor
{
    public class BFGlobalBootstrapConfigCreator : IBFProjectSetupStep, IBFSystemPrefabConsumer
    {
        private const string LogTag = "GlobalBootstrapper";
        private const string TargetPath = "Assets/Resources/BFTools";
        private const string AssetName = "GlobalBootstrapConfig.asset";

        private BFGlobalBootstrapperConfig config;

        public int Order => 100;
        public string DisplayName => "Global Bootstrap Config";

        [MenuItem("Assets/Create/BFTools/Systems/Config/Global Bootstrap Config", priority = BFSystemsMenuPriority.GlobalBootstrapper)]
        private static void Create()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFGlobalBootstrapperConfig>(TargetPath, AssetName);
        }

        public string Run()
        {
            config = BFEditorAssetUtility.CreateConfigAsset<BFGlobalBootstrapperConfig>(TargetPath, AssetName);
            return config != null ? $"{DisplayName} at {TargetPath}/{AssetName}" : null;
        }

        public void AssignSystemPrefabs(GameObject[] prefabs)
        {
            if (config == null || prefabs == null || prefabs.Length == 0)
                return;

            SerializedObject so = new SerializedObject(config);
            SerializedProperty arrayProp = so.FindProperty("systemPrefabs");
            if (arrayProp == null)
            {
                BFLogger.Error(LogTag, $"{nameof(BFGlobalBootstrapperConfig)} has no 'systemPrefabs' field.");
                return;
            }

            foreach (GameObject prefab in prefabs)
            {
                if (prefab == null || Contains(arrayProp, prefab))
                    continue;

                arrayProp.InsertArrayElementAtIndex(arrayProp.arraySize);
                arrayProp.GetArrayElementAtIndex(arrayProp.arraySize - 1).objectReferenceValue = prefab;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
        }

        private static bool Contains(SerializedProperty arrayProp, GameObject value)
        {
            for (int i = 0; i < arrayProp.arraySize; i++)
            {
                if (arrayProp.GetArrayElementAtIndex(i).objectReferenceValue == value)
                    return true;
            }

            return false;
        }
    }
}