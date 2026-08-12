using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using BFTools.Feedback.Hitstop;

namespace BFTools.Feedback.Hitstop.Tests
{
    public class BFHitstopConfigTests
    {
        private static BFHitstopConfig CreateConfig(params (string eventName, float timescale, float duration)[] entries)
        {
            BFHitstopConfig config = ScriptableObject.CreateInstance<BFHitstopConfig>();
            SerializedObject serializedConfig = new SerializedObject(config);
            SerializedProperty entriesProperty = serializedConfig.FindProperty("entries");

            for (int i = 0; i < entries.Length; i++)
            {
                entriesProperty.InsertArrayElementAtIndex(i);
                SerializedProperty element = entriesProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("eventName").stringValue = entries[i].eventName;
                element.FindPropertyRelative("timescale").floatValue = entries[i].timescale;
                element.FindPropertyRelative("duration").floatValue = entries[i].duration;
            }

            serializedConfig.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }

        [Test]
        public void TryGetEntry_ExistingEventName_ReturnsTrueWithMatchingValues()
        {
            BFHitstopConfig config = CreateConfig(("BigHit", 0.1f, 0.3f));

            bool found = config.TryGetEntry("BigHit", out BFHitstopEntry entry);

            Assert.IsTrue(found);
            Assert.AreEqual(0.1f, entry.timescale);
            Assert.AreEqual(0.3f, entry.duration);
        }

        [Test]
        public void TryGetEntry_MissingEventName_ReturnsFalse()
        {
            BFHitstopConfig config = CreateConfig(("BigHit", 0.1f, 0.3f));

            bool found = config.TryGetEntry("SmallHit", out _);

            Assert.IsFalse(found);
        }

        [Test]
        public void TryGetEntry_DuplicateEventNameInSameConfig_ReturnsFirstMatch()
        {
            BFHitstopConfig config = CreateConfig(
                ("BigHit", 0.1f, 0.1f),
                ("BigHit", 0.9f, 0.9f));

            config.TryGetEntry("BigHit", out BFHitstopEntry entry);

            Assert.AreEqual(0.1f, entry.timescale);
            Assert.AreEqual(0.1f, entry.duration);
        }

        [Test]
        public void Entries_ReflectsConfiguredList()
        {
            BFHitstopConfig config = CreateConfig(
                ("BigHit", 0.1f, 0.3f),
                ("SmallHit", 0.5f, 0.1f));

            Assert.AreEqual(2, config.Entries.Count);
            Assert.AreEqual("BigHit", config.Entries[0].eventName);
            Assert.AreEqual("SmallHit", config.Entries[1].eventName);
        }
    }
}