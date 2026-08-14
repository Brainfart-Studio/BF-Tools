using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace BFTools.Feedback.Vignette.Editor
{
    [CustomEditor(typeof(BFVignetteConfig))]
    public class BFVignetteConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty entriesProperty;
        private ReorderableList list;

        private void OnEnable()
        {
            entriesProperty = serializedObject.FindProperty("entries");

            list = new ReorderableList(serializedObject, entriesProperty, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Entries"),
                drawElementCallback = DrawElement,
                elementHeightCallback = GetElementHeight,
                onAddCallback = _ => AddEntry(),
                onRemoveCallback = ReorderableList.defaultBehaviours.DoRemoveButton
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = entriesProperty.GetArrayElementAtIndex(index);
            rect.y += 2f;
            rect.height -= 4f;
            EditorGUI.PropertyField(rect, element, GUIContent.none, true);
        }

        private float GetElementHeight(int index)
        {
            var element = entriesProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, true) + 4f;
        }

        private void AddEntry()
        {
            var config = (BFVignetteConfig)target;
            Undo.RecordObject(config, "Add Vignette Entry");
            config.AddEntry();
            EditorUtility.SetDirty(config);
            serializedObject.Update();
        }
    }
}