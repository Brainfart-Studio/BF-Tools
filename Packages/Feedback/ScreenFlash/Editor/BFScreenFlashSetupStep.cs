using UnityEditor;
using UnityEngine;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Core.Logger;
using BFTools.Core.ProjectSetup.Editor;

namespace BFTools.Feedback.ScreenFlash.Editor
{
    public class BFScreenFlashSetupStep : IBFProjectSetupStep, IBFSystemPrefabContributor
    {
        private const string LogTag = "ScreenFlash";
        private const string BasePrefabPath = "Packages/com.bftools.feedback/ScreenFlash/Prefabs/ScreenFlash.prefab";
        private const string PrefabTargetPath = "Assets/Prefabs/Feedback";
        private const string PrefabAssetName = "ScreenFlash.prefab";
        private const string ConfigTargetPath = "Assets/Configs/Feedback/ScreenFlash";
        private const string ConfigAssetName = "ScreenFlashConfig.asset";
        private const string DefaultEventName = "Default";

        private GameObject prefab;

        public int Order => 10;
        public string DisplayName => "Screen Flash";
        public GameObject SystemPrefab => prefab;

        public string Run()
        {
            prefab = BFEditorAssetUtility.CreatePrefabVariant(BasePrefabPath, PrefabTargetPath, PrefabAssetName);
            BFScreenFlashConfig config = BFEditorAssetUtility.CreateConfigAsset<BFScreenFlashConfig>(ConfigTargetPath, ConfigAssetName);

            if (config != null)
                SeedDefaultEntry(config);

            if (prefab == null || config == null)
                return null;

            AssignConfig(prefab, config);
            return $"{DisplayName} prefab + config";
        }

        private static void SeedDefaultEntry(BFScreenFlashConfig config)
        {
            if (config.Entries.Count > 0)
                return;

            SerializedObject so = new SerializedObject(config);
            SerializedProperty entriesProp = so.FindProperty("entries");
            if (entriesProp == null)
            {
                BFLogger.Error(LogTag, $"{nameof(BFScreenFlashConfig)} has no 'entries' field.");
                return;
            }

            entriesProp.InsertArrayElementAtIndex(0);
            SerializedProperty entry = entriesProp.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("eventName").stringValue = DefaultEventName;
            entry.FindPropertyRelative("flashColor").colorValue = Color.white;
            entry.FindPropertyRelative("duration").floatValue = 0.15f;
            entry.FindPropertyRelative("flashCount").intValue = 1;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
        }

        private static void AssignConfig(GameObject prefabInstance, BFScreenFlashConfig config)
        {
            BFScreenFlash component = prefabInstance.GetComponent<BFScreenFlash>();
            if (component == null)
            {
                BFLogger.Error(LogTag, $"'{PrefabAssetName}' has no {nameof(BFScreenFlash)} component.");
                return;
            }

            BFEditorAssetUtility.AssignConfigIfMissing(component, "configs", config);
        }
    }
}