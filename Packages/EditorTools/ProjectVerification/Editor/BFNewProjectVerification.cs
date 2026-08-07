using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using BFTools.Core.EditorAssetUtility.Editor;
using BFTools.Core.Logger;
using BFTools.EditorTools.ProjectVerification;

namespace BFTools.EditorTools.ProjectVerification.Editor
{
    public static class BFNewProjectVerification
    {
        private const string LogTag = "ProjectVerification";

        private const string ScenesFolder = "Assets/Scenes";
        private const string ScenePath = "Assets/Scenes/BFToolsTest.unity";

        [MenuItem("BF Tools/New Project Verification")]
        private static void Run()
        {
            SceneAsset existing = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (existing != null)
            {
                BFLogger.Warning(LogTag, $"Verification scene already exists at {ScenePath}. Opening it.");
                EditorSceneManager.OpenScene(ScenePath);
                Selection.activeObject = existing;
                EditorGUIUtility.PingObject(existing);
                return;
            }

            BFEditorAssetUtility.EnsureFolderExists(ScenesFolder);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BuildCamera();
            BuildEventSystem();
            Canvas canvas = BuildCanvas();
            BFProjectVerificationTrigger trigger = BuildTrigger();

            CreateButton(canvas.transform, "Hitstop", new Vector2(-330f, 0f), trigger.TriggerHitstop);
            CreateButton(canvas.transform, "Screen Shake", new Vector2(-110f, 0f), trigger.TriggerScreenShake);
            CreateButton(canvas.transform, "Screen Flash", new Vector2(110f, 0f), trigger.TriggerScreenFlash);
            CreateButton(canvas.transform, "Haptics", new Vector2(330f, 0f), trigger.TriggerHaptics);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            BFLogger.Info(LogTag, $"Created verification scene at {ScenePath}.");
            EditorUtility.DisplayDialog("BF Tools",
                $"Verification scene created at {ScenePath}.\n\n" +
                $"Each button fires its feedback event with eventName \"{BFProjectVerificationTrigger.EventName}\" " +
                "- add a matching entry to each feedback config to see it trigger. Enter Play Mode with this scene open to test.",
                "OK");
        }

        private static void BuildCamera()
        {
            if (Camera.main != null)
                return;

            GameObject go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            go.tag = "MainCamera";
            Undo.RegisterCreatedObjectUndo(go, "Create Main Camera");
        }

        private static void BuildEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
                return;

            GameObject go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Undo.RegisterCreatedObjectUndo(go, "Create EventSystem");
        }

        private static Canvas BuildCanvas()
        {
            GameObject go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(go, "Create Canvas");

            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            return canvas;
        }

        private static BFProjectVerificationTrigger BuildTrigger()
        {
            GameObject go = new GameObject("BFProjectVerificationTrigger", typeof(BFProjectVerificationTrigger));
            Undo.RegisterCreatedObjectUndo(go, "Create Verification Trigger");
            return go.GetComponent<BFProjectVerificationTrigger>();
        }

        private static void CreateButton(Transform parent, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction callback)
        {
            GameObject buttonGo = new GameObject($"{label} Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(parent, false);

            RectTransform rect = buttonGo.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200f, 60f);
            rect.anchoredPosition = anchoredPosition;

            Image image = buttonGo.GetComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            Button button = buttonGo.GetComponent<Button>();
            UnityEventTools.AddPersistentListener(button.onClick, callback);

            GameObject labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(buttonGo.transform, false);

            RectTransform labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI text = labelGo.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
        }
    }
}