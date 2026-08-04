using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using BFTools.Feedback.ScreenFlash;

namespace BFTools.Feedback.ScreenFlash.Tests
{
    public class BFScreenFlashConfigTests
    {
        private static BFScreenFlashConfig CreateConfig(params (string eventName, Color flashColor, float duration, int flashCount)[] entries)
        {
            BFScreenFlashConfig config = ScriptableObject.CreateInstance<BFScreenFlashConfig>();
            SerializedObject serializedConfig = new SerializedObject(config);
            SerializedProperty entriesProperty = serializedConfig.FindProperty("entries");

            for (int i = 0; i < entries.Length; i++)
            {
                entriesProperty.InsertArrayElementAtIndex(i);
                SerializedProperty element = entriesProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("eventName").stringValue = entries[i].eventName;
                element.FindPropertyRelative("flashColor").colorValue = entries[i].flashColor;
                element.FindPropertyRelative("duration").floatValue = entries[i].duration;
                element.FindPropertyRelative("flashCount").intValue = entries[i].flashCount;
            }

            serializedConfig.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }

        [Test]
        public void TryGetEntry_ExistingEventName_ReturnsTrueWithMatchingValues()
        {
            BFScreenFlashConfig config = CreateConfig(("Damage", Color.red, 0.4f, 2));

            bool found = config.TryGetEntry("Damage", out BFScreenFlashEntry entry);

            Assert.IsTrue(found);
            Assert.AreEqual(Color.red, entry.flashColor);
            Assert.AreEqual(0.4f, entry.duration);
            Assert.AreEqual(2, entry.flashCount);
        }

        [Test]
        public void TryGetEntry_MissingEventName_ReturnsFalse()
        {
            BFScreenFlashConfig config = CreateConfig(("Damage", Color.red, 0.4f, 2));

            bool found = config.TryGetEntry("Heal", out _);

            Assert.IsFalse(found);
        }

        [Test]
        public void TryGetEntry_DuplicateEventNameInSameConfig_ReturnsFirstMatch()
        {
            BFScreenFlashConfig config = CreateConfig(
                ("Damage", Color.red, 0.1f, 1),
                ("Damage", Color.white, 0.9f, 9));

            config.TryGetEntry("Damage", out BFScreenFlashEntry entry);

            Assert.AreEqual(Color.red, entry.flashColor);
            Assert.AreEqual(0.1f, entry.duration);
            Assert.AreEqual(1, entry.flashCount);
        }

        [Test]
        public void Entries_ReflectsConfiguredList()
        {
            BFScreenFlashConfig config = CreateConfig(
                ("Damage", Color.red, 0.4f, 2),
                ("Heal", Color.green, 0.2f, 1));

            Assert.AreEqual(2, config.Entries.Count);
            Assert.AreEqual("Damage", config.Entries[0].eventName);
            Assert.AreEqual("Heal", config.Entries[1].eventName);
        }
    }
}