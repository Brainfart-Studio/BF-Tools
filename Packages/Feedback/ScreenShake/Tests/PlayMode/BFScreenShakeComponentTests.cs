using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Feedback.ScreenShake;

namespace BFTools.Feedback.ScreenShake.PlayModeTests
{
    public class BFScreenShakeComponentTests
    {
        private GameObject go;
        private GameObject cameraGo;
        private List<Object> createdAssets;

        [SetUp]
        public void SetUp()
        {
            createdAssets = new List<Object>();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus<BFScreenShakeEvent>.Clear();

            if (go != null)
                Object.Destroy(go);

            if (cameraGo != null)
                Object.Destroy(cameraGo);

            foreach (Object asset in createdAssets)
                Object.Destroy(asset);

            BFLoggerTestUtility.ResetState();
        }

        private static BFScreenShakeConfig CreateConfig(params (string eventName, float amplitude, float duration)[] entries)
        {
            BFScreenShakeConfig config = ScriptableObject.CreateInstance<BFScreenShakeConfig>();
            List<BFScreenShakeEntry> list = new List<BFScreenShakeEntry>();
            foreach (var e in entries)
                list.Add(new BFScreenShakeEntry { eventName = e.eventName, amplitude = e.amplitude, duration = e.duration });

            typeof(BFScreenShakeConfig)
                .GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(config, list);

            return config;
        }

        private Camera CreateMainCamera()
        {
            cameraGo = new GameObject("MainCamera");
            cameraGo.tag = "MainCamera";
            return cameraGo.AddComponent<Camera>();
        }

        private BFScreenShake CreateScreenShake(params BFScreenShakeConfig[] configs)
        {
            go = new GameObject("ScreenShake");
            go.SetActive(false);

            BFScreenShake screenShake = go.AddComponent<BFScreenShake>();
            typeof(BFScreenShake)
                .GetField("configs", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(screenShake, new List<BFScreenShakeConfig>(configs));

            go.SetActive(true);
            return screenShake;
        }

        private static Dictionary<string, BFScreenShakeEntry> GetLookup(BFScreenShake screenShake)
        {
            return (Dictionary<string, BFScreenShakeEntry>)typeof(BFScreenShake)
                .GetField("lookup", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(screenShake);
        }

        private static Transform GetTarget(BFScreenShake screenShake)
        {
            return (Transform)typeof(BFScreenShake)
                .GetField("target", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(screenShake);
        }

        private static void InvokeOnSceneLoaded(BFScreenShake screenShake)
        {
            typeof(BFScreenShake)
                .GetMethod("OnSceneLoaded", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(screenShake, new object[] { default(Scene), LoadSceneMode.Single });
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
            CreateMainCamera();
            BFScreenShakeConfig configA = CreateConfig(("Explosion", 0.1f, 0.1f));
            BFScreenShakeConfig configB = CreateConfig(("Explosion", 0.8f, 0.4f), ("Footstep", 0.05f, 0.1f));
            createdAssets.Add(configA);
            createdAssets.Add(configB);

            BFScreenShake screenShake = CreateScreenShake(configA, configB);

            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Warning && e.Message.Contains("Duplicate eventName 'Explosion'")));

            Dictionary<string, BFScreenShakeEntry> lookup = GetLookup(screenShake);
            Assert.AreEqual(2, lookup.Count);
            Assert.AreEqual(0.8f, lookup["Explosion"].amplitude);
            Assert.AreEqual(0.4f, lookup["Explosion"].duration);
        }

        [Test]
        public void Fire_UnknownEventName_DoesNotThrowAndLogsTrace()
        {
            SpyLoggerSink spy = InitializeLogging();
            CreateMainCamera();
            BFScreenShakeConfig config = CreateConfig(("Explosion", 0.3f, 0.1f));
            createdAssets.Add(config);
            CreateScreenShake(config);

            Assert.DoesNotThrow(() => EventBus<BFScreenShakeEvent>.Fire(new BFScreenShakeEvent { eventName = "Footstep" }));

            Assert.IsTrue(spy.Entries.Exists(e => e.Message.Contains("No screen shake entry found for eventName 'Footstep'")));
        }

        [Test]
        public void OnEnable_NoMainCamera_LogsWarningAndFireSkipsGracefully()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFScreenShakeConfig config = CreateConfig(("Explosion", 0.3f, 0.1f));
            createdAssets.Add(config);
            CreateScreenShake(config);

            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Warning && e.Message.Contains("No main camera found to shake")));

            Assert.DoesNotThrow(() => EventBus<BFScreenShakeEvent>.Fire(new BFScreenShakeEvent { eventName = "Explosion" }));
        }

        [Test]
        public void Fire_KnownEventName_ImmediatelyMovesTarget()
        {
            Camera camera = CreateMainCamera();
            Vector3 originalPosition = camera.transform.localPosition;
            BFScreenShakeConfig config = CreateConfig(("Explosion", 0.5f, 5f));
            createdAssets.Add(config);
            CreateScreenShake(config);

            EventBus<BFScreenShakeEvent>.Fire(new BFScreenShakeEvent { eventName = "Explosion" });

            Assert.AreNotEqual(originalPosition, camera.transform.localPosition);
        }

        [UnityTest]
        public IEnumerator Fire_KnownEventName_RestoresOriginalPositionAfterDuration()
        {
            Camera camera = CreateMainCamera();
            Vector3 originalPosition = camera.transform.localPosition;
            BFScreenShakeConfig config = CreateConfig(("Explosion", 0.5f, 0.1f));
            createdAssets.Add(config);
            CreateScreenShake(config);

            EventBus<BFScreenShakeEvent>.Fire(new BFScreenShakeEvent { eventName = "Explosion" });

            yield return new WaitForSecondsRealtime(0.25f);

            Assert.AreEqual(originalPosition, camera.transform.localPosition);
        }

        [Test]
        public void OnDisable_UnsubscribesFromEventBus_SubsequentFireIsIgnored()
        {
            SpyLoggerSink spy = InitializeLogging();
            CreateMainCamera();
            BFScreenShakeConfig config = CreateConfig(("Explosion", 0.3f, 0.1f));
            createdAssets.Add(config);
            CreateScreenShake(config);

            go.SetActive(false);
            spy.Entries.Clear();

            EventBus<BFScreenShakeEvent>.Fire(new BFScreenShakeEvent { eventName = "Explosion" });

            Assert.IsFalse(spy.Entries.Exists(e => e.Message.Contains("Triggered shake")));
        }

        [Test]
        public void OnSceneLoaded_ReResolvesMainCameraTarget()
        {
            BFScreenShakeConfig config = CreateConfig(("Explosion", 0.3f, 0.1f));
            createdAssets.Add(config);
            BFScreenShake screenShake = CreateScreenShake(config);

            Assert.IsNull(GetTarget(screenShake));

            Camera camera = CreateMainCamera();
            InvokeOnSceneLoaded(screenShake);

            Assert.AreEqual(camera.transform, GetTarget(screenShake));
        }

        [UnityTest]
        public IEnumerator OnSceneLoaded_DuringActiveShake_CancelsShakeAndRestoresPreviousTarget()
        {
            Camera camera = CreateMainCamera();
            Vector3 originalPosition = camera.transform.localPosition;
            BFScreenShakeConfig config = CreateConfig(("Explosion", 0.5f, 1f));
            createdAssets.Add(config);
            BFScreenShake screenShake = CreateScreenShake(config);

            EventBus<BFScreenShakeEvent>.Fire(new BFScreenShakeEvent { eventName = "Explosion" });

            yield return null;

            Assert.AreNotEqual(originalPosition, camera.transform.localPosition,
                "Shake should have applied an offset before the scene load happens.");

            cameraGo.tag = "Untagged";
            InvokeOnSceneLoaded(screenShake);

            Assert.AreEqual(originalPosition, camera.transform.localPosition,
                "Cancelling the shake on scene load should restore the previous target's original position.");
            Assert.IsNull(GetTarget(screenShake));

            yield return null;

            Assert.AreEqual(originalPosition, camera.transform.localPosition,
                "The cancelled shake coroutine must not resume and move the old target on a later frame.");
        }
    }
}