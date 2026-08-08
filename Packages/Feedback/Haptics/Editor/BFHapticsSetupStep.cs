using UnityEditor;
using UnityEngine;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Core.Logger;
using BFTools.Core.ProjectSetup.Editor;

namespace BFTools.Feedback.Haptics.Editor
{
    public class BFHapticsSetupStep : IBFProjectSetupStep, IBFSystemPrefabContributor
    {
        private const string LogTag = "Haptics";
        private const string BasePrefabPath = "Packages/com.bftools.feedback/Haptics/Prefabs/Haptics.prefab";
        private const string PrefabTargetPath = "Assets/Prefabs/Feedback";
        private const string PrefabAssetName = "Haptics.prefab";
        private const string ConfigTargetPath = "Assets/Configs/Feedback/Haptics";
        private const string ConfigAssetName = "HapticsConfig.asset";
        private const string DefaultEventName = "Default";

        private GameObject prefab;

        public int Order => 10;
        public string DisplayName => "Haptics";
        public GameObject SystemPrefab => prefab;

        public string Run()
        {
            prefab = BFEditorAssetUtility.CreatePrefabVariant(BasePrefabPath, PrefabTargetPath, PrefabAssetName);
            BFHapticsConfig config = BFEditorAssetUtility.CreateConfigAsset<BFHapticsConfig>(ConfigTargetPath, ConfigAssetName);

            if (config != null)
                SeedDefaultEntry(config);

            if (prefab == null || config == null)
                return null;

            AssignConfig(prefab, config);
            return $"{DisplayName} prefab + config";
        }

        private static void SeedDefaultEntry(BFHapticsConfig config)
        {
            if (config.Entries.Count > 0)
                return;

            SerializedObject so = new SerializedObject(config);
            SerializedProperty entriesProp = so.FindProperty("entries");
            if (entriesProp == null)
            {
                BFLogger.Error(LogTag, $"{nameof(BFHapticsConfig)} has no 'entries' field.");
                return;
            }

            entriesProp.InsertArrayElementAtIndex(0);
            SerializedProperty entry = entriesProp.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("eventName").stringValue = DefaultEventName;
            entry.FindPropertyRelative("intensity").floatValue = 0.5f;
            entry.FindPropertyRelative("duration").floatValue = 0.2f;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
        }

        private static void AssignConfig(GameObject prefabInstance, BFHapticsConfig config)
        {
            BFHaptics component = prefabInstance.GetComponent<BFHaptics>();
            if (component == null)
            {
                BFLogger.Error(LogTag, $"'{PrefabAssetName}' has no {nameof(BFHaptics)} component.");
                return;
            }

            BFEditorAssetUtility.AssignConfigIfMissing(component, "configs", config);
        }
    }
}