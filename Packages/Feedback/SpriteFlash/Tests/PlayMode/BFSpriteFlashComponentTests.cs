using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Feedback.SpriteFlash;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Feedback.SpriteFlash.PlayModeTests
{
    public class BFSpriteFlashComponentTests
    {
        private GameObject go;
        private SpriteRenderer spriteRenderer;
        private List<Object> createdAssets;

        [SetUp]
        public void SetUp()
        {
            createdAssets = new List<Object>();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus<BFSpriteFlashEvent>.Clear();

            if (go != null)
                Object.Destroy(go);

            foreach (Object asset in createdAssets)
                Object.Destroy(asset);

            BFLoggerTestUtility.ResetState();
        }

        private static BFSpriteFlashConfig CreateConfig(params (string eventName, float interval, int flashCount)[] entries)
        {
            BFSpriteFlashConfig config = ScriptableObject.CreateInstance<BFSpriteFlashConfig>();
            List<BFSpriteFlashEntry> list = new List<BFSpriteFlashEntry>();
            foreach (var e in entries)
                list.Add(new BFSpriteFlashEntry { eventName = e.eventName, interval = e.interval, flashCount = e.flashCount });

            typeof(BFSpriteFlashConfig)
                .GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(config, list);

            return config;
        }

        private BFSpriteFlash CreateSpriteFlash(params BFSpriteFlashConfig[] configs)
        {
            go = new GameObject("SpriteFlash");
            go.SetActive(false);

            spriteRenderer = go.AddComponent<SpriteRenderer>();

            BFSpriteFlash spriteFlash = go.AddComponent<BFSpriteFlash>();
            typeof(BFSpriteFlash)
                .GetField("configs", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(spriteFlash, new List<BFSpriteFlashConfig>(configs));

            go.SetActive(true);
            return spriteFlash;
        }

        private static Dictionary<string, BFSpriteFlashEntry> GetLookup(BFSpriteFlash spriteFlash)
        {
            return (Dictionary<string, BFSpriteFlashEntry>)typeof(BFSpriteFlash)
                .GetField("lookup", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(spriteFlash);
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
            BFSpriteFlashConfig configA = CreateConfig(("Damage", 0.05f, 1));
            BFSpriteFlashConfig configB = CreateConfig(("Damage", 0.2f, 3), ("Heal", 0.1f, 1));
            createdAssets.Add(configA);
            createdAssets.Add(configB);

            BFSpriteFlash spriteFlash = CreateSpriteFlash(configA, configB);

            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Warning && e.Message.Contains("Duplicate eventName 'Damage'")));

            Dictionary<string, BFSpriteFlashEntry> lookup = GetLookup(spriteFlash);
            Assert.AreEqual(2, lookup.Count);
            Assert.AreEqual(0.2f, lookup["Damage"].interval);
            Assert.AreEqual(3, lookup["Damage"].flashCount);
        }

        [Test]
        public void Fire_UnknownEventName_DoesNotThrowAndLogsTrace()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFSpriteFlashConfig config = CreateConfig(("Damage", 0.05f, 1));
            createdAssets.Add(config);
            CreateSpriteFlash(config);

            Assert.DoesNotThrow(() => EventBus<BFSpriteFlashEvent>.Fire(new BFSpriteFlashEvent { eventName = "Missing" }));

            Assert.IsTrue(spy.Entries.Exists(e => e.Message.Contains("No sprite flash entry found for eventName 'Missing'")));
        }

        [Test]
        public void Fire_KnownEventName_ImmediatelyDisablesRenderer()
        {
            BFSpriteFlashConfig config = CreateConfig(("Damage", 5f, 1));
            createdAssets.Add(config);
            CreateSpriteFlash(config);

            EventBus<BFSpriteFlashEvent>.Fire(new BFSpriteFlashEvent { eventName = "Damage" });

            Assert.IsFalse(spriteRenderer.enabled);
        }

        [UnityTest]
        public IEnumerator Fire_KnownEventName_TogglesRendererAndEndsEnabled()
        {
            BFSpriteFlashConfig config = CreateConfig(("Damage", 0.05f, 2));
            createdAssets.Add(config);
            CreateSpriteFlash(config);

            EventBus<BFSpriteFlashEvent>.Fire(new BFSpriteFlashEvent { eventName = "Damage" });

            yield return new WaitForSecondsRealtime(0.3f);

            Assert.IsTrue(spriteRenderer.enabled);
        }

        [UnityTest]
        public IEnumerator Fire_WhileAlreadyActive_StopsPreviousFlashAndAppliesNewOne()
        {
            BFSpriteFlashConfig config = CreateConfig(
                ("Slow", 5f, 1),
                ("Fast", 0.02f, 1));
            createdAssets.Add(config);
            CreateSpriteFlash(config);

            EventBus<BFSpriteFlashEvent>.Fire(new BFSpriteFlashEvent { eventName = "Slow" });
            Assert.IsFalse(spriteRenderer.enabled);

            EventBus<BFSpriteFlashEvent>.Fire(new BFSpriteFlashEvent { eventName = "Fast" });

            yield return new WaitForSecondsRealtime(0.1f);

            Assert.IsTrue(spriteRenderer.enabled);
        }

        [Test]
        public void OnDisable_UnsubscribesFromEventBus_SubsequentFireIsIgnored()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFSpriteFlashConfig config = CreateConfig(("Damage", 0.05f, 1));
            createdAssets.Add(config);
            CreateSpriteFlash(config);

            go.SetActive(false);
            spy.Entries.Clear();
            spriteRenderer.enabled = true;

            EventBus<BFSpriteFlashEvent>.Fire(new BFSpriteFlashEvent { eventName = "Damage" });

            Assert.IsTrue(spriteRenderer.enabled);
            Assert.IsFalse(spy.Entries.Exists(e => e.Tags != null && System.Array.IndexOf(e.Tags, "SpriteFlash") >= 0));
        }

        [UnityTest]
        public IEnumerator OnDisable_DuringActiveFlash_ResetsRendererToEnabled()
        {
            BFSpriteFlashConfig config = CreateConfig(("Damage", 5f, 1));
            createdAssets.Add(config);
            CreateSpriteFlash(config);

            EventBus<BFSpriteFlashEvent>.Fire(new BFSpriteFlashEvent { eventName = "Damage" });

            yield return null;

            Assert.IsFalse(spriteRenderer.enabled,
                "Flash should still be mid-cycle before the component is disabled.");

            go.SetActive(false);

            Assert.IsTrue(spriteRenderer.enabled,
                "Disabling the component mid-flash should re-enable the sprite renderer.");
        }
    }
}