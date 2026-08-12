using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Feedback.ControllerLED;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Feedback.ControllerLED.Tests
{
    public class BFControllerLedManagerTests
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
            EventBus<BFControllerLedEvent>.Clear();

            if (go != null)
                Object.DestroyImmediate(go);

            foreach (Object asset in createdAssets)
                Object.DestroyImmediate(asset);

            BFLoggerTestUtility.ResetState();
        }

        private static BFControllerLedConfig CreateConfig(params (string eventName, Color color)[] entries)
        {
            BFControllerLedConfig config = ScriptableObject.CreateInstance<BFControllerLedConfig>();
            List<BFControllerLedEntry> list = new List<BFControllerLedEntry>();
            foreach (var e in entries)
                list.Add(new BFControllerLedEntry { eventName = e.eventName, color = e.color });

            typeof(BFControllerLedConfig)
                .GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(config, list);

            return config;
        }

        private BFControllerLedManager CreateManager(params BFControllerLedConfig[] configs)
        {
            go = new GameObject("ControllerLED");
            go.SetActive(false);

            BFControllerLedManager manager = go.AddComponent<BFControllerLedManager>();
            typeof(BFControllerLedManager)
                .GetField("configs", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(manager, new List<BFControllerLedConfig>(configs));

            go.SetActive(true);

            typeof(BFControllerLedManager)
                .GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(manager, null);

            return manager;
        }

        private static Dictionary<string, BFControllerLedEntry> GetLookup(BFControllerLedManager manager)
        {
            return (Dictionary<string, BFControllerLedEntry>)typeof(BFControllerLedManager)
                .GetField("lookup", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(manager);
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
            BFControllerLedConfig configA = CreateConfig(("LowHealth", Color.red));
            BFControllerLedConfig configB = CreateConfig(("LowHealth", Color.blue), ("PowerUp", Color.yellow));
            createdAssets.Add(configA);
            createdAssets.Add(configB);

            BFControllerLedManager manager = CreateManager(configA, configB);

            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Warning && e.Message.Contains("Duplicate eventName 'LowHealth'")));

            Dictionary<string, BFControllerLedEntry> lookup = GetLookup(manager);
            Assert.AreEqual(2, lookup.Count);
            Assert.AreEqual(Color.blue, lookup["LowHealth"].color);
            Assert.AreEqual(Color.yellow, lookup["PowerUp"].color);
        }

        [Test]
        public void Fire_UnknownEventName_DoesNotThrowAndLogsTrace()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFControllerLedConfig config = CreateConfig(("LowHealth", Color.red));
            createdAssets.Add(config);
            CreateManager(config);

            Assert.DoesNotThrow(() => EventBus<BFControllerLedEvent>.Fire(new BFControllerLedEvent { eventName = "Missing" }));

            Assert.IsTrue(spy.Entries.Exists(e => e.Message.Contains("No ControllerLED entry found for eventName 'Missing'")));
        }

        [Test]
        public void Fire_KnownEventName_NoGamepadConnected_DoesNotThrow()
        {
            BFControllerLedConfig config = CreateConfig(("LowHealth", Color.red));
            createdAssets.Add(config);
            CreateManager(config);

            Assert.DoesNotThrow(() => EventBus<BFControllerLedEvent>.Fire(new BFControllerLedEvent { eventName = "LowHealth" }));
        }

        [Test]
        public void SetColor_NoGamepadConnected_DoesNotThrow()
        {
            BFControllerLedManager manager = CreateManager();

            Assert.DoesNotThrow(() => manager.SetColor(Color.green));
        }

        [Test]
        public void OnDisable_UnsubscribesFromEventBus_SubsequentFireIsIgnored()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFControllerLedConfig config = CreateConfig(("LowHealth", Color.red));
            createdAssets.Add(config);
            BFControllerLedManager manager = CreateManager(config);

            go.SetActive(false);

            typeof(BFControllerLedManager)
                .GetMethod("OnDisable", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(manager, null);

            spy.Entries.Clear();

            EventBus<BFControllerLedEvent>.Fire(new BFControllerLedEvent { eventName = "LowHealth" });

            Assert.IsFalse(spy.Entries.Exists(e => e.Tags != null && System.Array.IndexOf(e.Tags, "ControllerLED") >= 0));
        }
    }
}