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

        [MenuItem("BF Tools/New Project Setup")]
        private static void Run()
        {
            BFEditorAssetUtility.CreateConfigAsset<BFLoggerConfig>(ResourcesPath, "BFLoggerConfig.asset");
            BFGlobalBootstrapperConfig bootstrapConfig =
                BFEditorAssetUtility.CreateConfigAsset<BFGlobalBootstrapperConfig>(ResourcesPath, "GlobalBootstrapConfig.asset");

            GameObject hitstopPrefab = SetupFeedbackModule<BFHitstopConfig, BFHitstop>(
                "Packages/com.bftools.feedback/Hitstop/Prefabs/Hitstop.prefab", "Hitstop.prefab",
                "Assets/Configs/Feedback/Hitstop", "HitstopConfig.asset");

            GameObject screenShakePrefab = SetupFeedbackModule<BFScreenShakeConfig, BFScreenShake>(
                "Packages/com.bftools.feedback/ScreenShake/Prefabs/ScreenShake.prefab", "ScreenShake.prefab",
                "Assets/Configs/Feedback/ScreenShake", "ScreenShakeConfig.asset");

            GameObject screenFlashPrefab = SetupFeedbackModule<BFScreenFlashConfig, BFScreenFlash>(
                "Packages/com.bftools.feedback/ScreenFlash/Prefabs/ScreenFlash.prefab", "ScreenFlash.prefab",
                "Assets/Configs/Feedback/ScreenFlash", "ScreenFlashConfig.asset");

            GameObject hapticsPrefab = SetupFeedbackModule<BFHapticsConfig, BFHaptics>(
                "Packages/com.bftools.feedback/Haptics/Prefabs/Haptics.prefab", "Haptics.prefab",
                "Assets/Configs/Feedback/Haptics", "HapticsConfig.asset");

            AssignSystemPrefabs(bootstrapConfig, hitstopPrefab, screenShakePrefab, screenFlashPrefab, hapticsPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            BFLogger.Info(LogTag, "New project setup complete.");
            EditorUtility.DisplayDialog("BF Tools",
                "Project setup complete.\n\nCreated Logger, Global Bootstrapper, and Hitstop/Screen Shake/Screen Flash/Haptics configs + prefabs, and wired everything into the Global Bootstrapper config.",
                "OK");
        }

        private static GameObject SetupFeedbackModule<TConfig, TComponent>(
            string basePrefabPath, string prefabAssetName,
            string configFolderPath, string configAssetName)
            where TConfig : ScriptableObject
            where TComponent : MonoBehaviour
        {
            GameObject prefab = BFEditorAssetUtility.CreatePrefabVariant(basePrefabPath, PrefabPath, prefabAssetName);
            TConfig config = BFEditorAssetUtility.CreateConfigAsset<TConfig>(configFolderPath, configAssetName);

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

        private static void AssignToObjectArray(SerializedObject serializedObject, string propertyName, Object value, bool applyImmediately = true)
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