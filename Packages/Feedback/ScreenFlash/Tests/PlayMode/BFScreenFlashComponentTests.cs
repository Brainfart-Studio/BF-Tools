using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Feedback.ScreenFlash;

namespace BFTools.Feedback.ScreenFlash.PlayModeTests
{
    public class BFScreenFlashComponentTests
    {
        private GameObject go;
        private Image flashImage;
        private List<Object> createdAssets;

        [SetUp]
        public void SetUp()
        {
            createdAssets = new List<Object>();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus<BFScreenFlashEvent>.Clear();

            if (go != null)
                Object.Destroy(go);

            foreach (Object asset in createdAssets)
                Object.Destroy(asset);

            BFLoggerTestUtility.ResetState();
        }

        private static BFScreenFlashConfig CreateConfig(params (string eventName, Color flashColor, float duration, int flashCount)[] entries)
        {
            BFScreenFlashConfig config = ScriptableObject.CreateInstance<BFScreenFlashConfig>();
            List<BFScreenFlashEntry> list = new List<BFScreenFlashEntry>();
            foreach (var e in entries)
                list.Add(new BFScreenFlashEntry { eventName = e.eventName, flashColor = e.flashColor, duration = e.duration, flashCount = e.flashCount });

            typeof(BFScreenFlashConfig)
                .GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(config, list);

            return config;
        }

        private BFScreenFlash CreateScreenFlash(params BFScreenFlashConfig[] configs)
        {
            go = new GameObject("ScreenFlash");
            go.SetActive(false);

            flashImage = go.AddComponent<Image>();
            flashImage.color = Color.white;

            BFScreenFlash screenFlash = go.AddComponent<BFScreenFlash>();
            typeof(BFScreenFlash)
                .GetField("configs", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(screenFlash, new List<BFScreenFlashConfig>(configs));
            typeof(BFScreenFlash)
                .GetField("flashImage", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(screenFlash, flashImage);

            go.SetActive(true);
            return screenFlash;
        }

        private static Dictionary<string, BFScreenFlashEntry> GetLookup(BFScreenFlash screenFlash)
        {
            return (Dictionary<string, BFScreenFlashEntry>)typeof(BFScreenFlash)
                .GetField("lookup", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(screenFlash);
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
            BFScreenFlashConfig configA = CreateConfig(("Damage", Color.red, 0.1f, 1));
            BFScreenFlashConfig configB = CreateConfig(("Damage", Color.white, 0.4f, 3), ("Heal", Color.green, 0.2f, 1));
            createdAssets.Add(configA);
            createdAssets.Add(configB);

            BFScreenFlash screenFlash = CreateScreenFlash(configA, configB);

            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Warning && e.Message.Contains("Duplicate eventName 'Damage'")));

            Dictionary<string, BFScreenFlashEntry> lookup = GetLookup(screenFlash);
            Assert.AreEqual(2, lookup.Count);
            Assert.AreEqual(Color.white, lookup["Damage"].flashColor);
            Assert.AreEqual(3, lookup["Damage"].flashCount);
        }

        [Test]
        public void Fire_UnknownEventName_DoesNotThrowAndLogsTrace()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFScreenFlashConfig config = CreateConfig(("Damage", Color.red, 0.1f, 1));
            createdAssets.Add(config);
            CreateScreenFlash(config);

            Assert.DoesNotThrow(() => EventBus<BFScreenFlashEvent>.Fire(new BFScreenFlashEvent { eventName = "Missing" }));

            Assert.IsTrue(spy.Entries.Exists(e => e.Message.Contains("No screen flash entry found for eventName 'Missing'")));
        }

        [Test]
        public void Fire_KnownEventName_ImmediatelyBeginsFading()
        {
            BFScreenFlashConfig config = CreateConfig(("Damage", Color.red, 5f, 1));
            createdAssets.Add(config);
            CreateScreenFlash(config);
            flashImage.color = Color.white;

            EventBus<BFScreenFlashEvent>.Fire(new BFScreenFlashEvent { eventName = "Damage" });

            Assert.Less(flashImage.color.a, 1f);
        }

        [UnityTest]
        public IEnumerator Fire_KnownEventName_FadesToTransparentByEndOfDuration()
        {
            BFScreenFlashConfig config = CreateConfig(("Damage", Color.red, 0.1f, 1));
            createdAssets.Add(config);
            CreateScreenFlash(config);

            EventBus<BFScreenFlashEvent>.Fire(new BFScreenFlashEvent { eventName = "Damage" });

            yield return new WaitForSecondsRealtime(0.3f);

            Assert.AreEqual(0f, flashImage.color.a);
            Assert.AreEqual(1f, flashImage.color.r);
            Assert.AreEqual(0f, flashImage.color.g);
        }

        [UnityTest]
        public IEnumerator Fire_WhileAlreadyActive_StopsPreviousFlashAndAppliesNewOne()
        {
            BFScreenFlashConfig config = CreateConfig(
                ("Slow", Color.red, 5f, 1),
                ("Fast", Color.blue, 0.05f, 1));
            createdAssets.Add(config);
            CreateScreenFlash(config);

            EventBus<BFScreenFlashEvent>.Fire(new BFScreenFlashEvent { eventName = "Slow" });
            Assert.Less(flashImage.color.a, 1f);

            EventBus<BFScreenFlashEvent>.Fire(new BFScreenFlashEvent { eventName = "Fast" });

            yield return new WaitForSecondsRealtime(0.2f);

            Assert.AreEqual(0f, flashImage.color.a);
            Assert.AreEqual(1f, flashImage.color.b);
        }

        [Test]
        public void OnDisable_UnsubscribesFromEventBus_SubsequentFireIsIgnored()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFScreenFlashConfig config = CreateConfig(("Damage", Color.red, 0.1f, 1));
            createdAssets.Add(config);
            CreateScreenFlash(config);

            go.SetActive(false);
            spy.Entries.Clear();
            flashImage.color = Color.white;

            EventBus<BFScreenFlashEvent>.Fire(new BFScreenFlashEvent { eventName = "Damage" });

            Assert.AreEqual(Color.white, flashImage.color);
            Assert.IsFalse(spy.Entries.Exists(e => e.Tags != null && System.Array.IndexOf(e.Tags, "ScreenFlash") >= 0));
        }

        [UnityTest]
        public IEnumerator OnDisable_DuringActiveFlash_ResetsAlphaToZero()
        {
            BFScreenFlashConfig config = CreateConfig(("Damage", Color.red, 1f, 1));
            createdAssets.Add(config);
            CreateScreenFlash(config);

            EventBus<BFScreenFlashEvent>.Fire(new BFScreenFlashEvent { eventName = "Damage" });

            yield return null;

            Assert.Greater(flashImage.color.a, 0f,
                "Flash should still be mid-fade before the component is disabled.");

            go.SetActive(false);

            Assert.AreEqual(0f, flashImage.color.a,
                "Disabling the component mid-flash should reset the flash image's alpha to zero.");
        }
    }
}