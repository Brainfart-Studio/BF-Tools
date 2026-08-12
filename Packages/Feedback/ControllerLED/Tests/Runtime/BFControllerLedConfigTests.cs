using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using BFTools.Feedback.ControllerLED;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Feedback.ControllerLED.Tests
{
    public class BFControllerLedConfigTests
    {
        private static BFControllerLedConfig CreateConfig(params (string eventName, Color color)[] entries)
        {
            BFControllerLedConfig config = ScriptableObject.CreateInstance<BFControllerLedConfig>();
            SerializedObject serializedConfig = new SerializedObject(config);
            SerializedProperty entriesProperty = serializedConfig.FindProperty("entries");

            for (int i = 0; i < entries.Length; i++)
            {
                entriesProperty.InsertArrayElementAtIndex(i);
                SerializedProperty element = entriesProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("eventName").stringValue = entries[i].eventName;
                element.FindPropertyRelative("color").colorValue = entries[i].color;
            }

            serializedConfig.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }

        [Test]
        public void TryGetEntry_ExistingEventName_ReturnsTrueWithMatchingValues()
        {
            BFControllerLedConfig config = CreateConfig(("LowHealth", Color.red));

            bool found = config.TryGetEntry("LowHealth", out BFControllerLedEntry entry);

            Assert.IsTrue(found);
            Assert.AreEqual(Color.red, entry.color);
        }

        [Test]
        public void TryGetEntry_MissingEventName_ReturnsFalse()
        {
            BFControllerLedConfig config = CreateConfig(("LowHealth", Color.red));

            bool found = config.TryGetEntry("PowerUp", out _);

            Assert.IsFalse(found);
        }

        [Test]
        public void TryGetEntry_DuplicateEventNameInSameConfig_ReturnsFirstMatch()
        {
            BFControllerLedConfig config = CreateConfig(
                ("LowHealth", Color.red),
                ("LowHealth", Color.blue));

            config.TryGetEntry("LowHealth", out BFControllerLedEntry entry);

            Assert.AreEqual(Color.red, entry.color);
        }

        [Test]
        public void Entries_ReflectsConfiguredList()
        {
            BFControllerLedConfig config = CreateConfig(
                ("LowHealth", Color.red),
                ("PowerUp", Color.yellow));

            Assert.AreEqual(2, config.Entries.Count);
            Assert.AreEqual("LowHealth", config.Entries[0].eventName);
            Assert.AreEqual("PowerUp", config.Entries[1].eventName);
        }
    }
}