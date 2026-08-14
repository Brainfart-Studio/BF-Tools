using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Feedback.Vignette;

namespace BFTools.Feedback.Vignette.Tests
{
    public class BFVignetteConfigTests
    {
        private static BFVignetteConfig CreateConfig(params BFVignetteEntry[] entries)
        {
            BFVignetteConfig config = ScriptableObject.CreateInstance<BFVignetteConfig>();
            typeof(BFVignetteConfig)
                .GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(config, new List<BFVignetteEntry>(entries));

            return config;
        }

        private static BFVignetteEntry MakeEntry(string eventName, float intensity = 1f)
        {
            return new BFVignetteEntry { eventName = eventName, intensity = intensity };
        }

        [Test]
        public void TryGetEntry_ExistingEventName_ReturnsTrueWithMatchingValues()
        {
            BFVignetteConfig config = CreateConfig(MakeEntry("Explosion", 0.6f));

            bool found = config.TryGetEntry("Explosion", out BFVignetteEntry entry);

            Assert.IsTrue(found);
            Assert.AreEqual(0.6f, entry.intensity);
        }

        [Test]
        public void TryGetEntry_MissingEventName_ReturnsFalse()
        {
            BFVignetteConfig config = CreateConfig(MakeEntry("Explosion"));

            bool found = config.TryGetEntry("Footstep", out _);

            Assert.IsFalse(found);
        }

        [Test]
        public void TryGetEntry_DuplicateEventNameInSameConfig_ReturnsFirstMatch()
        {
            BFVignetteConfig config = CreateConfig(MakeEntry("Explosion", 0.1f), MakeEntry("Explosion", 0.9f));

            config.TryGetEntry("Explosion", out BFVignetteEntry entry);

            Assert.AreEqual(0.1f, entry.intensity);
        }

        [Test]
        public void Entries_ReflectsConfiguredList()
        {
            BFVignetteConfig config = CreateConfig(MakeEntry("Explosion"), MakeEntry("Footstep"));

            Assert.AreEqual(2, config.Entries.Count);
            Assert.AreEqual("Explosion", config.Entries[0].eventName);
            Assert.AreEqual("Footstep", config.Entries[1].eventName);
        }

        [Test]
        public void AddEntry_AppendsEntryWithExpectedDefaults()
        {
            BFVignetteConfig config = ScriptableObject.CreateInstance<BFVignetteConfig>();

            config.AddEntry();

            Assert.AreEqual(1, config.Entries.Count);

            BFVignetteEntry entry = config.Entries[0];
            Assert.AreEqual(1f, entry.intensity);
            Assert.AreEqual(Color.red, entry.color);
            Assert.AreEqual(0.75f, entry.radius);
            Assert.AreEqual(0.5f, entry.softness);
            Assert.AreEqual(1f, entry.roundness);
            Assert.AreEqual(0.5f, entry.duration);
            Assert.IsNotNull(entry.intensityCurve);
            Assert.Greater(entry.intensityCurve.length, 0);
        }

        [Test]
        public void AddEntry_CalledTwice_AppendsIndependentEntries()
        {
            BFVignetteConfig config = ScriptableObject.CreateInstance<BFVignetteConfig>();

            config.AddEntry();
            config.AddEntry();
            config.Entries[0].intensity = 0.2f;

            Assert.AreEqual(2, config.Entries.Count);
            Assert.AreEqual(0.2f, config.Entries[0].intensity);
            Assert.AreEqual(1f, config.Entries[1].intensity);
        }
    }
}