using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using BFTools.Feedback.Haptics;

namespace BFTools.Feedback.Haptics.Tests
{
    public class BFHapticsConfigTests
    {
        private static BFHapticsConfig CreateConfig(params (string eventName, float intensity, float duration)[] entries)
        {
            BFHapticsConfig config = ScriptableObject.CreateInstance<BFHapticsConfig>();
            SerializedObject serializedConfig = new SerializedObject(config);
            SerializedProperty entriesProperty = serializedConfig.FindProperty("entries");

            for (int i = 0; i < entries.Length; i++)
            {
                entriesProperty.InsertArrayElementAtIndex(i);
                SerializedProperty element = entriesProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("eventName").stringValue = entries[i].eventName;
                element.FindPropertyRelative("intensity").floatValue = entries[i].intensity;
                element.FindPropertyRelative("duration").floatValue = entries[i].duration;
            }

            serializedConfig.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }

        [Test]
        public void TryGetEntry_ExistingEventName_ReturnsTrueWithMatchingValues()
        {
            BFHapticsConfig config = CreateConfig(("Hit", 0.5f, 0.2f));

            bool found = config.TryGetEntry("Hit", out BFHapticsEntry entry);

            Assert.IsTrue(found);
            Assert.AreEqual(0.5f, entry.intensity);
            Assert.AreEqual(0.2f, entry.duration);
        }

        [Test]
        public void TryGetEntry_MissingEventName_ReturnsFalse()
        {
            BFHapticsConfig config = CreateConfig(("Hit", 0.5f, 0.2f));

            bool found = config.TryGetEntry("Explosion", out _);

            Assert.IsFalse(found);
        }

        [Test]
        public void TryGetEntry_DuplicateEventNameInSameConfig_ReturnsFirstMatch()
        {
            BFHapticsConfig config = CreateConfig(
                ("Hit", 0.1f, 0.1f),
                ("Hit", 0.9f, 0.9f));

            config.TryGetEntry("Hit", out BFHapticsEntry entry);

            Assert.AreEqual(0.1f, entry.intensity);
            Assert.AreEqual(0.1f, entry.duration);
        }

        [Test]
        public void Entries_ReflectsConfiguredList()
        {
            BFHapticsConfig config = CreateConfig(
                ("Hit", 0.1f, 0.1f),
                ("Explosion", 1f, 0.5f));

            Assert.AreEqual(2, config.Entries.Count);
            Assert.AreEqual("Hit", config.Entries[0].eventName);
            Assert.AreEqual("Explosion", config.Entries[1].eventName);
        }
    }
}