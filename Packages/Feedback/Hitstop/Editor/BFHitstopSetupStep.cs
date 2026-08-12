using UnityEditor;
using UnityEngine;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Core.Logger;
using BFTools.Core.ProjectSetup.Editor;

namespace BFTools.Feedback.Hitstop.Editor
{
    public class BFHitstopSetupStep : IBFProjectSetupStep, IBFSystemPrefabContributor
    {
        private const string LogTag = "Hitstop";
        private const string BasePrefabPath = "Packages/com.bftools.feedback/Hitstop/Prefabs/Hitstop.prefab";
        private const string PrefabTargetPath = "Assets/Prefabs/Feedback";
        private const string PrefabAssetName = "Hitstop.prefab";
        private const string ConfigTargetPath = "Assets/Configs/Feedback/Hitstop";
        private const string ConfigAssetName = "HitstopConfig.asset";
        private const string DefaultEventName = "Default";

        private GameObject prefab;

        public int Order => 10;
        public string DisplayName => "Hitstop";
        public GameObject SystemPrefab => prefab;

        public string Run()
        {
            prefab = BFEditorAssetUtility.CreatePrefabVariant(BasePrefabPath, PrefabTargetPath, PrefabAssetName);
            BFHitstopConfig config = BFEditorAssetUtility.CreateConfigAsset<BFHitstopConfig>(ConfigTargetPath, ConfigAssetName);

            if (config != null)
                SeedDefaultEntry(config);

            if (prefab == null || config == null)
                return null;

            AssignConfig(prefab, config);
            return $"{DisplayName} prefab + config";
        }

        private static void SeedDefaultEntry(BFHitstopConfig config)
        {
            if (config.Entries.Count > 0)
                return;

            SerializedObject so = new SerializedObject(config);
            SerializedProperty entriesProp = so.FindProperty("entries");
            if (entriesProp == null)
            {
                BFLogger.Error(LogTag, $"{nameof(BFHitstopConfig)} has no 'entries' field.");
                return;
            }

            entriesProp.InsertArrayElementAtIndex(0);
            SerializedProperty entry = entriesProp.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("eventName").stringValue = DefaultEventName;
            entry.FindPropertyRelative("timescale").floatValue = 0.05f;
            entry.FindPropertyRelative("duration").floatValue = 0.15f;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
        }

        private static void AssignConfig(GameObject prefabInstance, BFHitstopConfig config)
        {
            BFHitstop component = prefabInstance.GetComponent<BFHitstop>();
            if (component == null)
            {
                BFLogger.Error(LogTag, $"'{PrefabAssetName}' has no {nameof(BFHitstop)} component.");
                return;
            }

            BFEditorAssetUtility.AssignConfigIfMissing(component, "configs", config);
        }
    }
}