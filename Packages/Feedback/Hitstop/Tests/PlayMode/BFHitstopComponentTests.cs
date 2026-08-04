using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Feedback.Hitstop;

namespace BFTools.Feedback.Hitstop.PlayModeTests
{
    public class BFHitstopComponentTests
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
            EventBus<BFHitstopEvent>.Clear();

            if (go != null)
                Object.Destroy(go);

            foreach (Object asset in createdAssets)
                Object.Destroy(asset);

            Time.timeScale = 1f;
            BFLoggerTestUtility.ResetState();
        }

        private static BFHitstopConfig CreateConfig(params (string eventName, float timescale, float duration)[] entries)
        {
            BFHitstopConfig config = ScriptableObject.CreateInstance<BFHitstopConfig>();
            List<BFHitstopEntry> list = new List<BFHitstopEntry>();
            foreach (var e in entries)
                list.Add(new BFHitstopEntry { eventName = e.eventName, timescale = e.timescale, duration = e.duration });

            typeof(BFHitstopConfig)
                .GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(config, list);

            return config;
        }

        private BFHitstop CreateHitstop(params BFHitstopConfig[] configs)
        {
            go = new GameObject("Hitstop");
            go.SetActive(false);

            BFHitstop hitstop = go.AddComponent<BFHitstop>();
            typeof(BFHitstop)
                .GetField("configs", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(hitstop, new List<BFHitstopConfig>(configs));

            go.SetActive(true);
            return hitstop;
        }

        private static Dictionary<string, BFHitstopEntry> GetLookup(BFHitstop hitstop)
        {
            return (Dictionary<string, BFHitstopEntry>)typeof(BFHitstop)
                .GetField("lookup", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(hitstop);
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
            BFHitstopConfig configA = CreateConfig(("BigHit", 0.2f, 0.1f));
            BFHitstopConfig configB = CreateConfig(("BigHit", 0.8f, 0.4f), ("SmallHit", 0.5f, 0.2f));
            createdAssets.Add(configA);
            createdAssets.Add(configB);

            BFHitstop hitstop = CreateHitstop(configA, configB);

            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Warning && e.Message.Contains("Duplicate eventName 'BigHit'")));

            Dictionary<string, BFHitstopEntry> lookup = GetLookup(hitstop);
            Assert.AreEqual(2, lookup.Count);
            Assert.AreEqual(0.8f, lookup["BigHit"].timescale);
            Assert.AreEqual(0.4f, lookup["BigHit"].duration);
        }

        [Test]
        public void Fire_UnknownEventName_DoesNotThrowAndLogsTrace()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFHitstopConfig config = CreateConfig(("BigHit", 0.2f, 0.1f));
            createdAssets.Add(config);
            CreateHitstop(config);

            Assert.DoesNotThrow(() => EventBus<BFHitstopEvent>.Fire(new BFHitstopEvent { eventName = "Missing" }));

            Assert.IsTrue(spy.Entries.Exists(e => e.Message.Contains("No hitstop entry found for eventName 'Missing'")));
        }

        [Test]
        public void Fire_KnownEventName_SetsTimeScaleImmediately()
        {
            BFHitstopConfig config = CreateConfig(("BigHit", 0.2f, 5f));
            createdAssets.Add(config);
            CreateHitstop(config);

            EventBus<BFHitstopEvent>.Fire(new BFHitstopEvent { eventName = "BigHit" });

            Assert.AreEqual(0.2f, Time.timeScale);
        }

        [UnityTest]
        public IEnumerator Fire_KnownEventName_RestoresTimeScaleAfterDuration()
        {
            BFHitstopConfig config = CreateConfig(("BigHit", 0.2f, 0.1f));
            createdAssets.Add(config);
            CreateHitstop(config);

            EventBus<BFHitstopEvent>.Fire(new BFHitstopEvent { eventName = "BigHit" });
            Assert.AreEqual(0.2f, Time.timeScale);

            yield return new WaitForSecondsRealtime(0.25f);

            Assert.AreEqual(1f, Time.timeScale);
        }

        [UnityTest]
        public IEnumerator Fire_WhileAlreadyActive_StopsPreviousRestoreAndAppliesNewOne()
        {
            BFHitstopConfig config = CreateConfig(
                ("Slow", 0.1f, 5f),
                ("Fast", 0.5f, 0.05f));
            createdAssets.Add(config);
            CreateHitstop(config);

            EventBus<BFHitstopEvent>.Fire(new BFHitstopEvent { eventName = "Slow" });
            Assert.AreEqual(0.1f, Time.timeScale);

            EventBus<BFHitstopEvent>.Fire(new BFHitstopEvent { eventName = "Fast" });
            Assert.AreEqual(0.5f, Time.timeScale);

            yield return new WaitForSecondsRealtime(0.2f);

            Assert.AreEqual(1f, Time.timeScale);
        }

        [Test]
        public void OnDisable_UnsubscribesFromEventBus_SubsequentFireIsIgnored()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFHitstopConfig config = CreateConfig(("BigHit", 0.2f, 0.1f));
            createdAssets.Add(config);
            CreateHitstop(config);

            go.SetActive(false);
            spy.Entries.Clear();
            Time.timeScale = 1f;

            EventBus<BFHitstopEvent>.Fire(new BFHitstopEvent { eventName = "BigHit" });

            Assert.AreEqual(1f, Time.timeScale);
            Assert.AreEqual(0, spy.Entries.Count);
        }
    }
}