using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using BFTools.Feedback.SpriteFlash;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Feedback.SpriteFlash.Tests
{
    public class BFSpriteFlashConfigTests
    {
        private static BFSpriteFlashConfig CreateConfig(params (string eventName, float interval, int flashCount)[] entries)
        {
            BFSpriteFlashConfig config = ScriptableObject.CreateInstance<BFSpriteFlashConfig>();
            SerializedObject serializedConfig = new SerializedObject(config);
            SerializedProperty entriesProperty = serializedConfig.FindProperty("entries");

            for (int i = 0; i < entries.Length; i++)
            {
                entriesProperty.InsertArrayElementAtIndex(i);
                SerializedProperty element = entriesProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("eventName").stringValue = entries[i].eventName;
                element.FindPropertyRelative("interval").floatValue = entries[i].interval;
                element.FindPropertyRelative("flashCount").intValue = entries[i].flashCount;
            }

            serializedConfig.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }

        [Test]
        public void TryGetEntry_ExistingEventName_ReturnsTrueWithMatchingValues()
        {
            BFSpriteFlashConfig config = CreateConfig(("Damage", 0.05f, 4));

            bool found = config.TryGetEntry("Damage", out BFSpriteFlashEntry entry);

            Assert.IsTrue(found);
            Assert.AreEqual(0.05f, entry.interval);
            Assert.AreEqual(4, entry.flashCount);
        }

        [Test]
        public void TryGetEntry_MissingEventName_ReturnsFalse()
        {
            BFSpriteFlashConfig config = CreateConfig(("Damage", 0.05f, 4));

            bool found = config.TryGetEntry("Heal", out _);

            Assert.IsFalse(found);
        }

        [Test]
        public void TryGetEntry_DuplicateEventNameInSameConfig_ReturnsFirstMatch()
        {
            BFSpriteFlashConfig config = CreateConfig(
                ("Damage", 0.05f, 4),
                ("Damage", 0.2f, 1));

            config.TryGetEntry("Damage", out BFSpriteFlashEntry entry);

            Assert.AreEqual(0.05f, entry.interval);
            Assert.AreEqual(4, entry.flashCount);
        }

        [Test]
        public void Entries_ReflectsConfiguredList()
        {
            BFSpriteFlashConfig config = CreateConfig(
                ("Damage", 0.05f, 4),
                ("Heal", 0.1f, 2));

            Assert.AreEqual(2, config.Entries.Count);
            Assert.AreEqual("Damage", config.Entries[0].eventName);
            Assert.AreEqual("Heal", config.Entries[1].eventName);
        }
    }
}