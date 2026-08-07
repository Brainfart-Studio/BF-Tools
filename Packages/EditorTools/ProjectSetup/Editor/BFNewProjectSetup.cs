using System;
using UnityEditor;
using UnityEngine;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Core.Logger;
using BFTools.Systems.GlobalBootstrapper;
using BFTools.Feedback.Hitstop;
using BFTools.Feedback.ScreenShake;
using BFTools.Feedback.ScreenFlash;
using BFTools.Feedback.Haptics;

namespace BFTools.EditorTools.ProjectSetup.Editor
{
    public static class BFNewProjectSetup
    {
        private const string LogTag = "ProjectSetup";

        private const string ResourcesPath = "Assets/Resources/BFTools";
        private const string PrefabPath = "Assets/Prefabs/Feedback";
        private const string DefaultEventName = "Default";

        [MenuItem("BF Tools/New Project Setup")]
        private static void Run()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFLoggerConfig>(ResourcesPath, "BFLoggerConfig.asset");
            BFGlobalBootstrapperConfig bootstrapConfig =
                BFEditorAssetUtility.CreateConfigAsset<BFGlobalBootstrapperConfig>(ResourcesPath, "GlobalBootstrapConfig.asset");

            GameObject hitstopPrefab = SetupFeedbackModule<BFHitstopConfig, BFHitstop>(
                "Packages/com.bftools.feedback/Hitstop/Prefabs/Hitstop.prefab", "Hitstop.prefab",
                "Assets/Configs/Feedback/Hitstop", "HitstopConfig.asset",
                config => SeedDefaultEntry(config, config.Entries.Count, entry =>
                {
                    entry.FindPropertyRelative("timescale").floatValue = 0.05f;
                    entry.FindPropertyRelative("duration").floatValue = 0.15f;
                }));

            GameObject screenShakePrefab = SetupFeedbackModule<BFScreenShakeConfig, BFScreenShake>(
                "Packages/com.bftools.feedback/ScreenShake/Prefabs/ScreenShake.prefab", "ScreenShake.prefab",
                "Assets/Configs/Feedback/ScreenShake", "ScreenShakeConfig.asset",
                config => SeedDefaultEntry(config, config.Entries.Count, entry =>
                {
                    entry.FindPropertyRelative("amplitude").floatValue = 0.3f;
                    entry.FindPropertyRelative("duration").floatValue = 0.2f;
                }));

            GameObject screenFlashPrefab = SetupFeedbackModule<BFScreenFlashConfig, BFScreenFlash>(
                "Packages/com.bftools.feedback/ScreenFlash/Prefabs/ScreenFlash.prefab", "ScreenFlash.prefab",
                "Assets/Configs/Feedback/ScreenFlash", "ScreenFlashConfig.asset",
                config => SeedDefaultEntry(config, config.Entries.Count, entry =>
                {
                    entry.FindPropertyRelative("flashColor").colorValue = Color.white;
                    entry.FindPropertyRelative("duration").floatValue = 0.15f;
                    entry.FindPropertyRelative("flashCount").intValue = 1;
                }));

            GameObject hapticsPrefab = SetupFeedbackModule<BFHapticsConfig, BFHaptics>(
                "Packages/com.bftools.feedback/Haptics/Prefabs/Haptics.prefab", "Haptics.prefab",
                "Assets/Configs/Feedback/Haptics", "HapticsConfig.asset",
                config => SeedDefaultEntry(config, config.Entries.Count, entry =>
                {
                    entry.FindPropertyRelative("intensity").floatValue = 0.5f;
                    entry.FindPropertyRelative("duration").floatValue = 0.2f;
                }));

            AssignSystemPrefabs(bootstrapConfig, hitstopPrefab, screenShakePrefab, screenFlashPrefab, hapticsPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            BFLogger.Info(LogTag, "New project setup complete.");
            EditorUtility.DisplayDialog("BF Tools",
                $"Project setup complete.\n\nCreated Logger, Global Bootstrapper, and Hitstop/Screen Shake/Screen Flash/Haptics configs + prefabs, wired everything into the Global Bootstrapper config, and seeded each config with a \"{DefaultEventName}\" entry.",
                "OK");
        }

        private static GameObject SetupFeedbackModule<TConfig, TComponent>(
            string basePrefabPath, string prefabAssetName,
            string configFolderPath, string configAssetName,
            Action<TConfig> seedDefaults)
            where TConfig : ScriptableObject
            where TComponent : MonoBehaviour
        {
            GameObject prefab = BFEditorAssetUtility.CreatePrefabVariant(basePrefabPath, PrefabPath, prefabAssetName);
            TConfig config = BFEditorAssetUtility.CreateConfigAsset<TConfig>(configFolderPath, configAssetName);

            if (config != null)
                seedDefaults?.Invoke(config);

            if (prefab == null || config == null)
                return prefab;

            TComponent component = prefab.GetComponent<TComponent>();
            if (component == null)
            {
                BFLogger.Error(LogTag, $"'{prefabAssetName}' has no {typeof(TComponent).Name} component.");
                return prefab;
            }

            AssignToObjectArray(new SerializedObject(component), "configs", config);
            return prefab;
        }

        private static void SeedDefaultEntry(ScriptableObject config, int existingEntryCount, Action<SerializedProperty> configureEntry)
        {
            if (existingEntryCount > 0)
                return;

            SerializedObject so = new SerializedObject(config);
            SerializedProperty entriesProp = so.FindProperty("entries");
            if (entriesProp == null)
            {
                BFLogger.Error(LogTag, $"{config.GetType().Name} has no 'entries' field.");
                return;
            }

            entriesProp.InsertArrayElementAtIndex(0);
            SerializedProperty entry = entriesProp.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("eventName").stringValue = DefaultEventName;
            configureEntry(entry);

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
        }

        private static void AssignSystemPrefabs(BFGlobalBootstrapperConfig bootstrapConfig, params GameObject[] prefabs)
        {
            if (bootstrapConfig == null)
                return;

            SerializedObject so = new SerializedObject(bootstrapConfig);
            foreach (GameObject prefab in prefabs)
                AssignToObjectArray(so, "systemPrefabs", prefab, applyImmediately: false);

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(bootstrapConfig);
        }

        private static void AssignToObjectArray(SerializedObject serializedObject, string propertyName, UnityEngine.Object value, bool applyImmediately = true)
        {
            if (value == null)
                return;

            SerializedProperty arrayProp = serializedObject.FindProperty(propertyName);
            if (arrayProp == null)
            {
                BFLogger.Error(LogTag, $"{serializedObject.targetObject.GetType().Name} has no '{propertyName}' field.");
                return;
            }

            for (int i = 0; i < arrayProp.arraySize; i++)
            {
                if (arrayProp.GetArrayElementAtIndex(i).objectReferenceValue == value)
                    return;
            }

            arrayProp.InsertArrayElementAtIndex(arrayProp.arraySize);
            arrayProp.GetArrayElementAtIndex(arrayProp.arraySize - 1).objectReferenceValue = value;

            if (applyImmediately)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(serializedObject.targetObject);
            }
        }
    }
}