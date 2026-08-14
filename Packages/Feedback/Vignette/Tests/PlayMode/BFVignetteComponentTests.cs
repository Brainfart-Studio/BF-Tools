using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Feedback.Vignette;

namespace BFTools.Feedback.Vignette.PlayModeTests
{
    public class BFVignetteComponentTests
    {
        private GameObject go;
        private List<Object> createdObjects;

        [SetUp]
        public void SetUp()
        {
            createdObjects = new List<Object>();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus<BFVignetteEvent>.Clear();

            if (go != null)
                Object.Destroy(go);

            foreach (Object obj in createdObjects)
                Object.Destroy(obj);

            BFLoggerTestUtility.ResetState();
        }

        private static BFVignetteEntry MakeEntry(string eventName, Color color, float duration = 1f)
        {
            return new BFVignetteEntry
            {
                eventName = eventName,
                intensity = 1f,
                color = color,
                duration = duration,
                intensityCurve = AnimationCurve.Constant(0f, 1f, 1f),
                radius = 0.5f,
                softness = 0.2f,
                roundness = 1f
            };
        }

        private BFVignetteConfig CreateConfig(params BFVignetteEntry[] entries)
        {
            BFVignetteConfig config = ScriptableObject.CreateInstance<BFVignetteConfig>();
            typeof(BFVignetteConfig)
                .GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(config, new List<BFVignetteEntry>(entries));

            createdObjects.Add(config);
            return config;
        }

        private Image CreateImage()
        {
            GameObject imageGo = new GameObject("Image", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageGo.transform.SetParent(go.transform, false);
            return imageGo.GetComponent<Image>();
        }

        private BFVignette CreateVignette(int layerCount, params BFVignetteConfig[] configs)
        {
            go = new GameObject("Vignette");
            go.SetActive(false);

            List<Image> images = new List<Image>();
            for (int i = 0; i < layerCount; i++)
                images.Add(CreateImage());

            BFVignette vignette = go.AddComponent<BFVignette>();
            typeof(BFVignette).GetField("configs", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(vignette, new List<BFVignetteConfig>(configs));
            typeof(BFVignette).GetField("vignetteImages", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(vignette, images);

            go.SetActive(true);
            return vignette;
        }

        private static List<Image> GetImages(BFVignette vignette)
        {
            return (List<Image>)typeof(BFVignette)
                .GetField("vignetteImages", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(vignette);
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
        public void OnEnable_NoLayerImagesAssigned_LogsWarning()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFVignetteConfig config = CreateConfig(MakeEntry("Explosion", Color.red));

            CreateVignette(0, config);

            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Warning && e.Message.Contains("No vignette layer images assigned")));
        }

        [Test]
        public void Fire_UnknownEventName_DoesNotThrowAndLogsTrace()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFVignetteConfig config = CreateConfig(MakeEntry("Explosion", Color.red));
            CreateVignette(1, config);

            Assert.DoesNotThrow(() => EventBus<BFVignetteEvent>.Fire(new BFVignetteEvent { eventName = "Footstep" }));

            Assert.IsTrue(spy.Entries.Exists(e => e.Message.Contains("No vignette entry found for eventName 'Footstep'")));
        }

        [Test]
        public void Fire_UnimplementedBlendMode_LogsWarningAndStillApplies()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFVignetteEntry entry = MakeEntry("Explosion", Color.red);
            entry.blendMode = BFVignetteBlendMode.Multiply;
            BFVignetteConfig config = CreateConfig(entry);
            BFVignette vignette = CreateVignette(1, config);

            EventBus<BFVignetteEvent>.Fire(new BFVignetteEvent { eventName = "Explosion" });

            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Warning && e.Message.Contains("not implemented yet")));
            Assert.Greater(GetImages(vignette)[0].color.a, 0f);
        }

        [Test]
        public void Fire_KnownEventName_AppliesEntryColorToLayerImage()
        {
            BFVignetteConfig config = CreateConfig(MakeEntry("Explosion", Color.red));
            BFVignette vignette = CreateVignette(1, config);

            EventBus<BFVignetteEvent>.Fire(new BFVignetteEvent { eventName = "Explosion" });

            Color appliedColor = GetImages(vignette)[0].color;
            Assert.AreEqual(1f, appliedColor.r);
            Assert.AreEqual(0f, appliedColor.g);
            Assert.AreEqual(0f, appliedColor.b);
            Assert.AreEqual(1f, appliedColor.a, 0.001f);
        }

        [Test]
        public void Fire_TwoSimultaneousEvents_UseSeparateLayers()
        {
            BFVignetteConfig config = CreateConfig(
                MakeEntry("Explosion", Color.red),
                MakeEntry("Freeze", Color.blue));
            BFVignette vignette = CreateVignette(2, config);

            EventBus<BFVignetteEvent>.Fire(new BFVignetteEvent { eventName = "Explosion" });
            EventBus<BFVignetteEvent>.Fire(new BFVignetteEvent { eventName = "Freeze" });

            List<Image> images = GetImages(vignette);
            Assert.AreEqual(1f, images[0].color.r);
            Assert.AreEqual(1f, images[1].color.b);
        }

        [Test]
        public void Fire_SecondEventWithNoFreeLayers_ReusesOldestLayer()
        {
            BFVignetteConfig config = CreateConfig(
                MakeEntry("Explosion", Color.red, duration: 5f),
                MakeEntry("Freeze", Color.blue, duration: 5f));
            BFVignette vignette = CreateVignette(1, config);

            EventBus<BFVignetteEvent>.Fire(new BFVignetteEvent { eventName = "Explosion" });
            EventBus<BFVignetteEvent>.Fire(new BFVignetteEvent { eventName = "Freeze" });

            Color appliedColor = GetImages(vignette)[0].color;
            Assert.AreEqual(1f, appliedColor.b);
            Assert.AreEqual(0f, appliedColor.r);
        }

        [UnityTest]
        public IEnumerator OnDisable_DuringActiveVignette_ResetsLayerAlphaToZero()
        {
            BFVignetteConfig config = CreateConfig(MakeEntry("Explosion", Color.red, duration: 5f));
            BFVignette vignette = CreateVignette(1, config);

            EventBus<BFVignetteEvent>.Fire(new BFVignetteEvent { eventName = "Explosion" });

            yield return null;

            Assert.Greater(GetImages(vignette)[0].color.a, 0f);

            go.SetActive(false);

            Assert.AreEqual(0f, GetImages(vignette)[0].color.a);
        }
    }
}