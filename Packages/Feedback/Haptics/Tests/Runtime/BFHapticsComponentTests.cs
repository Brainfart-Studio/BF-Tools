using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Feedback.Haptics;

namespace BFTools.Feedback.Haptics.Tests
{
    public class BFHapticsComponentTests
    {
        private GameObject go;
        private List<Object> createdAssets;

        [SetUp]
        public void SetUp()
        {
            createdAssets = new List<Object>();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus<BFHapticsEvent>.Clear();

            if (go != null)
                Object.DestroyImmediate(go);

            foreach (Object asset in createdAssets)
                Object.DestroyImmediate(asset);

            BFLoggerTestUtility.ResetState();
        }

        private static BFHapticsConfig CreateConfig(params (string eventName, float intensity, float duration)[] entries)
        {
            BFHapticsConfig config = ScriptableObject.CreateInstance<BFHapticsConfig>();
            List<BFHapticsEntry> list = new List<BFHapticsEntry>();
            foreach (var e in entries)
                list.Add(new BFHapticsEntry { eventName = e.eventName, intensity = e.intensity, duration = e.duration });

            typeof(BFHapticsConfig)
                .GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(config, list);

            return config;
        }

        private BFHaptics CreateHaptics(params BFHapticsConfig[] configs)
        {
            go = new GameObject("Haptics");
            go.SetActive(false);

            BFHaptics haptics = go.AddComponent<BFHaptics>();
            typeof(BFHaptics)
                .GetField("configs", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(haptics, new List<BFHapticsConfig>(configs));

            go.SetActive(true);

            typeof(BFHaptics)
                .GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(haptics, null);

            return haptics;
        }

        private static Dictionary<string, BFHapticsEntry> GetLookup(BFHaptics haptics)
        {
            return (Dictionary<string, BFHapticsEntry>)typeof(BFHaptics)
                .GetField("lookup", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(haptics);
        }

        private static SpyLoggerSink InitializeLogging()
        {
            BFLoggerConfig config = ScriptableObject.CreateInstance<BFLoggerConfig>();
            typeof(BFLoggerConfig)
                .GetField("globalMinimumLevel", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(config, LogLevel.Trace);

            SpyLoggerSink spy = new SpyLoggerSink();
            BFLogger.Initialize(config, spy);
            return spy;
        }

        [Test]
        public void OnEnable_DuplicateEventNameAcrossConfigs_LastConfigWinsAndLogsWarning()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFHapticsConfig configA = CreateConfig(("Hit", 0.2f, 0.1f));
            BFHapticsConfig configB = CreateConfig(("Hit", 0.8f, 0.4f), ("Explosion", 1f, 0.5f));
            createdAssets.Add(configA);
            createdAssets.Add(configB);

            BFHaptics haptics = CreateHaptics(configA, configB);

            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Warning && e.Message.Contains("Duplicate eventName 'Hit'")));

            Dictionary<string, BFHapticsEntry> lookup = GetLookup(haptics);
            Assert.AreEqual(2, lookup.Count);
            Assert.AreEqual(0.8f, lookup["Hit"].intensity);
            Assert.AreEqual(0.4f, lookup["Hit"].duration);
            Assert.AreEqual(1f, lookup["Explosion"].intensity);
        }

        [Test]
        public void Fire_UnknownEventName_DoesNotThrowAndLogsTrace()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFHapticsConfig config = CreateConfig(("Hit", 0.5f, 0.2f));
            createdAssets.Add(config);
            CreateHaptics(config);

            Assert.DoesNotThrow(() => EventBus<BFHapticsEvent>.Fire(new BFHapticsEvent { eventName = "Missing" }));

            Assert.IsTrue(spy.Entries.Exists(e => e.Message.Contains("No haptics entry found for eventName 'Missing'")));
        }

        [Test]
        public void Fire_KnownEventName_NoGamepadConnected_DoesNotThrowAndLogsSkip()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFHapticsConfig config = CreateConfig(("Hit", 0.5f, 0.2f));
            createdAssets.Add(config);
            CreateHaptics(config);

            Assert.DoesNotThrow(() => EventBus<BFHapticsEvent>.Fire(new BFHapticsEvent { eventName = "Hit" }));

            Assert.IsTrue(spy.Entries.Exists(e => e.Message.Contains("No gamepad connected, skipping haptics trigger")));
        }

        [Test]
        public void OnDisable_UnsubscribesFromEventBus_SubsequentFireIsIgnored()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFHapticsConfig config = CreateConfig(("Hit", 0.5f, 0.2f));
            createdAssets.Add(config);
            BFHaptics haptics = CreateHaptics(config);

            go.SetActive(false);

            typeof(BFHaptics)
                .GetMethod("OnDisable", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(haptics, null);

            spy.Entries.Clear();

            EventBus<BFHapticsEvent>.Fire(new BFHapticsEvent { eventName = "Hit" });

            Assert.IsFalse(spy.Entries.Exists(e => e.Tags != null && System.Array.IndexOf(e.Tags, "Haptics") >= 0));
        }
    }
}