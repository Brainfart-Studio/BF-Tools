using System;
using UnityEditor;
using UnityEngine;

namespace BFTools.Visuals.Palette.Editor
{
    [CustomEditor(typeof(BFPalette))]
    public class BFPaletteEditor : UnityEditor.Editor
    {
        private SerializedProperty configProperty;
        private SerializedProperty selectedEntryNameProperty;

        private void OnEnable()
        {
            configProperty = serializedObject.FindProperty("config");
            selectedEntryNameProperty = serializedObject.FindProperty("selectedEntryName");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(configProperty);

            var config = configProperty.objectReferenceValue as BFPaletteConfig;
            if (config == null)
            {
                EditorGUILayout.HelpBox("Assign a Palette Config to choose an entry.", MessageType.Info);
            }
            else
            {
                DrawEntryDropdown(config);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawEntryDropdown(BFPaletteConfig config)
        {
            var entries = config.Entries;

            if (entries.Count == 0)
            {
                EditorGUILayout.HelpBox("Palette Config has no entries.", MessageType.Info);
                return;
            }

            var names = new string[entries.Count];
            for (int i = 0; i < entries.Count; i++)
                names[i] = entries[i].name;

            int currentIndex = Array.IndexOf(names, selectedEntryNameProperty.stringValue);
            int newIndex = EditorGUILayout.Popup("Selected Entry", currentIndex, names);

            if (newIndex != currentIndex && newIndex >= 0)
            {
                selectedEntryNameProperty.stringValue = names[newIndex];
                ApplyToTarget(names[newIndex]);
            }
        }

        private void ApplyToTarget(string entryName)
        {
            var palette = (BFPalette)target;
            var spriteRenderer = palette.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                return;

            Undo.RecordObject(spriteRenderer, "Apply Palette Entry");
            palette.Apply(entryName);
            EditorUtility.SetDirty(spriteRenderer);
        }
    }
}