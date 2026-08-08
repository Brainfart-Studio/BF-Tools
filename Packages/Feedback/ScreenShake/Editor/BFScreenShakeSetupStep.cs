using UnityEditor;
using UnityEngine;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Core.Logger;
using BFTools.Core.ProjectSetup.Editor;

namespace BFTools.Feedback.ScreenShake.Editor
{
    public class BFScreenShakeSetupStep : IBFProjectSetupStep, IBFSystemPrefabContributor
    {
        private const string LogTag = "ScreenShake";
        private const string BasePrefabPath = "Packages/com.bftools.feedback/ScreenShake/Prefabs/ScreenShake.prefab";
        private const string PrefabTargetPath = "Assets/Prefabs/Feedback";
        private const string PrefabAssetName = "ScreenShake.prefab";
        private const string ConfigTargetPath = "Assets/Configs/Feedback/ScreenShake";
        private const string ConfigAssetName = "ScreenShakeConfig.asset";
        private const string DefaultEventName = "Default";

        private GameObject prefab;

        public int Order => 10;
        public string DisplayName => "Screen Shake";
        public GameObject SystemPrefab => prefab;

        public string Run()
        {
            prefab = BFEditorAssetUtility.CreatePrefabVariant(BasePrefabPath, PrefabTargetPath, PrefabAssetName);
            BFScreenShakeConfig config = BFEditorAssetUtility.CreateConfigAsset<BFScreenShakeConfig>(ConfigTargetPath, ConfigAssetName);

            if (config != null)
                SeedDefaultEntry(config);

            if (prefab == null || config == null)
                return null;

            AssignConfig(prefab, config);
            return $"{DisplayName} prefab + config";
        }

        private static void SeedDefaultEntry(BFScreenShakeConfig config)
        {
            if (config.Entries.Count > 0)
                return;

            SerializedObject so = new SerializedObject(config);
            SerializedProperty entriesProp = so.FindProperty("entries");
            if (entriesProp == null)
            {
                BFLogger.Error(LogTag, $"{nameof(BFScreenShakeConfig)} has no 'entries' field.");
                return;
            }

            entriesProp.InsertArrayElementAtIndex(0);
            SerializedProperty entry = entriesProp.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("eventName").stringValue = DefaultEventName;
            entry.FindPropertyRelative("amplitude").floatValue = 0.3f;
            entry.FindPropertyRelative("duration").floatValue = 0.2f;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
        }

        private static void AssignConfig(GameObject prefabInstance, BFScreenShakeConfig config)
        {
            BFScreenShake component = prefabInstance.GetComponent<BFScreenShake>();
            if (component == null)
            {
                BFLogger.Error(LogTag, $"'{PrefabAssetName}' has no {nameof(BFScreenShake)} component.");
                return;
            }

            SerializedObject so = new SerializedObject(component);
            SerializedProperty arrayProp = so.FindProperty("configs");
            if (arrayProp == null)
            {
                BFLogger.Error(LogTag, $"{nameof(BFScreenShake)} has no 'configs' field.");
                return;
            }

            for (int i = 0; i < arrayProp.arraySize; i++)
            {
                if (arrayProp.GetArrayElementAtIndex(i).objectReferenceValue == config)
                    return;
            }

            arrayProp.InsertArrayElementAtIndex(arrayProp.arraySize);
            arrayProp.GetArrayElementAtIndex(arrayProp.arraySize - 1).objectReferenceValue = config;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(component);
        }
    }
}