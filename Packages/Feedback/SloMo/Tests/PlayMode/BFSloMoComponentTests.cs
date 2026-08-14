using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Feedback.SloMo;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Feedback.SloMo.PlayModeTests
{
    public class BFSloMoComponentTests
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
            EventBus<BFSloMoEvent>.Clear();

            if (go != null)
                Object.Destroy(go);

            foreach (Object asset in createdAssets)
                Object.Destroy(asset);

            Time.timeScale = 1f;
            BFLoggerTestUtility.ResetState();
        }

        private static BFSloMoConfig CreateConfig(params (string eventName, float duration, AnimationCurve curve)[] entries)
        {
            BFSloMoConfig config = ScriptableObject.CreateInstance<BFSloMoConfig>();
            List<BFSloMoEntry> list = new List<BFSloMoEntry>();
            foreach (var e in entries)
                list.Add(new BFSloMoEntry { eventName = e.eventName, duration = e.duration, curve = e.curve });

            typeof(BFSloMoConfig)
                .GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(config, list);

            return config;
        }

        private BFSloMo CreateSloMo(params BFSloMoConfig[] configs)
        {
            go = new GameObject("SloMo");
            go.SetActive(false);

            BFSloMo sloMo = go.AddComponent<BFSloMo>();
            typeof(BFSloMo)
                .GetField("configs", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(sloMo, new List<BFSloMoConfig>(configs));

            go.SetActive(true);
            return sloMo;
        }

        private static Dictionary<string, BFSloMoEntry> GetLookup(BFSloMo sloMo)
        {
            return (Dictionary<string, BFSloMoEntry>)typeof(BFSloMo)
                .GetField("lookup", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(sloMo);
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
            AnimationCurve curveA = AnimationCurve.Linear(0f, 0.2f, 0.1f, 0.2f);
            AnimationCurve curveB = AnimationCurve.Linear(0f, 0.8f, 0.4f, 0.8f);
            BFSloMoConfig configA = CreateConfig(("BigSlow", 0.1f, curveA));
            BFSloMoConfig configB = CreateConfig(("BigSlow", 0.4f, curveB), ("SmallSlow", 0.2f, curveB));
            createdAssets.Add(configA);
            createdAssets.Add(configB);

            BFSloMo sloMo = CreateSloMo(configA, configB);

            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Warning && e.Message.Contains("Duplicate eventName 'BigSlow'")));

            Dictionary<string, BFSloMoEntry> lookup = GetLookup(sloMo);
            Assert.AreEqual(2, lookup.Count);
            Assert.AreEqual(0.4f, lookup["BigSlow"].duration);
        }

        [Test]
        public void Fire_UnknownEventName_DoesNotThrowAndLogsTrace()
        {
            SpyLoggerSink spy = InitializeLogging();
            AnimationCurve curve = AnimationCurve.Linear(0f, 0.2f, 0.1f, 0.2f);
            BFSloMoConfig config = CreateConfig(("BigSlow", 0.1f, curve));
            createdAssets.Add(config);
            CreateSloMo(config);

            Assert.DoesNotThrow(() => EventBus<BFSloMoEvent>.Fire(new BFSloMoEvent { eventName = "Missing" }));

            Assert.IsTrue(spy.Entries.Exists(e => e.Message.Contains("No slo-mo entry found for eventName 'Missing'")));
        }

        [UnityTest]
        public IEnumerator Fire_KnownEventName_SetsTimeScaleFromCurve()
        {
            AnimationCurve curve = AnimationCurve.Linear(0f, 0.2f, 5f, 0.2f);
            BFSloMoConfig config = CreateConfig(("BigSlow", 5f, curve));
            createdAssets.Add(config);
            CreateSloMo(config);

            EventBus<BFSloMoEvent>.Fire(new BFSloMoEvent { eventName = "BigSlow" });
            yield return null;

            Assert.AreEqual(0.2f, Time.timeScale);
        }

        [UnityTest]
        public IEnumerator Fire_KnownEventName_RestoresTimeScaleAfterDuration()
        {
            AnimationCurve curve = AnimationCurve.Linear(0f, 0.2f, 0.1f, 0.2f);
            BFSloMoConfig config = CreateConfig(("BigSlow", 0.1f, curve));
            createdAssets.Add(config);
            CreateSloMo(config);

            EventBus<BFSloMoEvent>.Fire(new BFSloMoEvent { eventName = "BigSlow" });
            yield return null;
            Assert.AreEqual(0.2f, Time.timeScale);

            yield return new WaitForSecondsRealtime(0.25f);

            Assert.AreEqual(1f, Time.timeScale);
        }

        [UnityTest]
        public IEnumerator Fire_WhileAlreadyActive_StopsPreviousAndAppliesNewOne()
        {
            AnimationCurve slowCurve = AnimationCurve.Linear(0f, 0.1f, 5f, 0.1f);
            AnimationCurve fastCurve = AnimationCurve.Linear(0f, 0.5f, 0.05f, 0.5f);
            BFSloMoConfig config = CreateConfig(
                ("Slow", 5f, slowCurve),
                ("Fast", 0.05f, fastCurve));
            createdAssets.Add(config);
            CreateSloMo(config);

            EventBus<BFSloMoEvent>.Fire(new BFSloMoEvent { eventName = "Slow" });
            yield return null;
            Assert.AreEqual(0.1f, Time.timeScale);

            EventBus<BFSloMoEvent>.Fire(new BFSloMoEvent { eventName = "Fast" });
            yield return null;
            Assert.AreEqual(0.5f, Time.timeScale);

            yield return new WaitForSecondsRealtime(0.2f);

            Assert.AreEqual(1f, Time.timeScale);
        }

        [Test]
        public void OnDisable_UnsubscribesFromEventBus_SubsequentFireIsIgnored()
        {
            SpyLoggerSink spy = InitializeLogging();
            AnimationCurve curve = AnimationCurve.Linear(0f, 0.2f, 0.1f, 0.2f);
            BFSloMoConfig config = CreateConfig(("BigSlow", 0.1f, curve));
            createdAssets.Add(config);
            CreateSloMo(config);

            go.SetActive(false);
            spy.Entries.Clear();
            Time.timeScale = 1f;

            EventBus<BFSloMoEvent>.Fire(new BFSloMoEvent { eventName = "BigSlow" });

            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsFalse(spy.Entries.Exists(e => e.Message.Contains("Triggered slo-mo")));
        }

        [UnityTest]
        public IEnumerator OnDisable_DuringActiveSloMo_RestoresTimeScale()
        {
            AnimationCurve curve = AnimationCurve.Linear(0f, 0.2f, 1f, 0.2f);
            BFSloMoConfig config = CreateConfig(("BigSlow", 1f, curve));
            createdAssets.Add(config);
            CreateSloMo(config);

            EventBus<BFSloMoEvent>.Fire(new BFSloMoEvent { eventName = "BigSlow" });
            yield return null;

            Assert.AreEqual(0.2f, Time.timeScale,
                "SloMo should have applied the curve's timescale before the component is disabled.");

            go.SetActive(false);

            Assert.AreEqual(1f, Time.timeScale,
                "Disabling the component mid-slo-mo should restore Time.timeScale to 1.");
        }
    }
}