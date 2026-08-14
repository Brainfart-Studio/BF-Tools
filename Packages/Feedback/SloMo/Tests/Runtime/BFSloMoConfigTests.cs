using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using BFTools.Feedback.SloMo;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Feedback.SloMo.Tests
{
    public class BFSloMoConfigTests
    {
        private static BFSloMoConfig CreateConfig(params (string eventName, float duration, AnimationCurve curve)[] entries)
        {
            BFSloMoConfig config = ScriptableObject.CreateInstance<BFSloMoConfig>();
            SerializedObject serializedConfig = new SerializedObject(config);
            SerializedProperty entriesProperty = serializedConfig.FindProperty("entries");

            for (int i = 0; i < entries.Length; i++)
            {
                entriesProperty.InsertArrayElementAtIndex(i);
                SerializedProperty element = entriesProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("eventName").stringValue = entries[i].eventName;
                element.FindPropertyRelative("duration").floatValue = entries[i].duration;
                element.FindPropertyRelative("curve").animationCurveValue = entries[i].curve;
            }

            serializedConfig.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }

        [Test]
        public void TryGetEntry_ExistingEventName_ReturnsTrueWithMatchingValues()
        {
            AnimationCurve curve = AnimationCurve.Linear(0f, 1f, 0.3f, 0.2f);
            BFSloMoConfig config = CreateConfig(("BigSlow", 0.3f, curve));

            bool found = config.TryGetEntry("BigSlow", out BFSloMoEntry entry);

            Assert.IsTrue(found);
            Assert.AreEqual(0.3f, entry.duration);
            Assert.AreEqual(1f, entry.curve.Evaluate(0f));
        }

        [Test]
        public void TryGetEntry_MissingEventName_ReturnsFalse()
        {
            AnimationCurve curve = AnimationCurve.Linear(0f, 1f, 0.3f, 0.2f);
            BFSloMoConfig config = CreateConfig(("BigSlow", 0.3f, curve));

            bool found = config.TryGetEntry("SmallSlow", out _);

            Assert.IsFalse(found);
        }

        [Test]
        public void TryGetEntry_DuplicateEventNameInSameConfig_ReturnsFirstMatch()
        {
            AnimationCurve curveA = AnimationCurve.Linear(0f, 1f, 0.1f, 0.1f);
            AnimationCurve curveB = AnimationCurve.Linear(0f, 1f, 0.9f, 0.9f);
            BFSloMoConfig config = CreateConfig(
                ("BigSlow", 0.1f, curveA),
                ("BigSlow", 0.9f, curveB));

            config.TryGetEntry("BigSlow", out BFSloMoEntry entry);

            Assert.AreEqual(0.1f, entry.duration);
        }

        [Test]
        public void Entries_ReflectsConfiguredList()
        {
            AnimationCurve curveA = AnimationCurve.Linear(0f, 1f, 0.3f, 0.2f);
            AnimationCurve curveB = AnimationCurve.Linear(0f, 1f, 0.1f, 0.5f);
            BFSloMoConfig config = CreateConfig(
                ("BigSlow", 0.3f, curveA),
                ("SmallSlow", 0.1f, curveB));

            Assert.AreEqual(2, config.Entries.Count);
            Assert.AreEqual("BigSlow", config.Entries[0].eventName);
            Assert.AreEqual("SmallSlow", config.Entries[1].eventName);
        }
    }
}