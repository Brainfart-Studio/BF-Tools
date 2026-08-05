using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using BFTools.Feedback.ScreenShake;

namespace BFTools.Feedback.ScreenShake.Tests
{
    public class BFScreenShakeConfigTests
    {
        private static BFScreenShakeConfig CreateConfig(params (string eventName, float amplitude, float duration)[] entries)
        {
            BFScreenShakeConfig config = ScriptableObject.CreateInstance<BFScreenShakeConfig>();
            SerializedObject serializedConfig = new SerializedObject(config);
            SerializedProperty entriesProperty = serializedConfig.FindProperty("entries");

            for (int i = 0; i < entries.Length; i++)
            {
                entriesProperty.InsertArrayElementAtIndex(i);
                SerializedProperty element = entriesProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("eventName").stringValue = entries[i].eventName;
                element.FindPropertyRelative("amplitude").floatValue = entries[i].amplitude;
                element.FindPropertyRelative("duration").floatValue = entries[i].duration;
            }

            serializedConfig.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }

        [Test]
        public void TryGetEntry_ExistingEventName_ReturnsTrueWithMatchingValues()
        {
            BFScreenShakeConfig config = CreateConfig(("Explosion", 0.3f, 0.6f));

            bool found = config.TryGetEntry("Explosion", out BFScreenShakeEntry entry);

            Assert.IsTrue(found);
            Assert.AreEqual(0.3f, entry.amplitude);
            Assert.AreEqual(0.6f, entry.duration);
        }

        [Test]
        public void TryGetEntry_MissingEventName_ReturnsFalse()
        {
            BFScreenShakeConfig config = CreateConfig(("Explosion", 0.3f, 0.6f));

            bool found = config.TryGetEntry("Footstep", out _);

            Assert.IsFalse(found);
        }

        [Test]
        public void TryGetEntry_DuplicateEventNameInSameConfig_ReturnsFirstMatch()
        {
            BFScreenShakeConfig config = CreateConfig(
                ("Explosion", 0.1f, 0.1f),
                ("Explosion", 0.9f, 0.9f));

            config.TryGetEntry("Explosion", out BFScreenShakeEntry entry);

            Assert.AreEqual(0.1f, entry.amplitude);
            Assert.AreEqual(0.1f, entry.duration);
        }

        [Test]
        public void Entries_ReflectsConfiguredList()
        {
            BFScreenShakeConfig config = CreateConfig(
                ("Explosion", 0.3f, 0.6f),
                ("Footstep", 0.05f, 0.1f));

            Assert.AreEqual(2, config.Entries.Count);
            Assert.AreEqual("Explosion", config.Entries[0].eventName);
            Assert.AreEqual("Footstep", config.Entries[1].eventName);
        }
    }
}