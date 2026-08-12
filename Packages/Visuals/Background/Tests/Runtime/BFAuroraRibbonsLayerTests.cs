using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Visuals.Background;

namespace BFTools.Visuals.Background.Tests
{
    public class BFAuroraRibbonsLayerTests
    {
        private GameObject parentGo;
        private List<Object> createdAssets;

        [SetUp]
        public void SetUp()
        {
            parentGo = new GameObject("Parent");
            createdAssets = new List<Object>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(parentGo);

            foreach (Object asset in createdAssets)
                if (asset != null)
                    Object.DestroyImmediate(asset);

            BFLoggerTestUtility.ResetState();
        }

        private BFAuroraRibbonsLayerConfig CreateConfig()
        {
            BFAuroraRibbonsLayerConfig config = ScriptableObject.CreateInstance<BFAuroraRibbonsLayerConfig>();
            createdAssets.Add(config);
            return config;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            target.GetType()
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(target, value);
        }

        private static T GetField<T>(object target, string fieldName)
        {
            return (T)target.GetType()
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(target);
        }

        private static SpyLoggerSink InitializeLogging()
        {
            BFLoggerConfig loggerConfig = ScriptableObject.CreateInstance<BFLoggerConfig>();
            typeof(BFLoggerConfig)
                .GetField("globalMinimumLevel", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(loggerConfig, LogLevel.Trace);

            SpyLoggerSink spy = new SpyLoggerSink();
            BFLogger.Initialize(loggerConfig, spy);
            return spy;
        }

        [Test]
        public void CreateLayer_ReturnsBFAuroraRibbonsLayer()
        {
            BFAuroraRibbonsLayerConfig config = CreateConfig();

            IBFBackgroundLayer layer = config.CreateLayer();

            Assert.IsInstanceOf<BFAuroraRibbonsLayer>(layer);
        }

        [Test]
        public void Init_EmptyRibbonColors_LogsError()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFAuroraRibbonsLayerConfig config = CreateConfig();
            SetField(config, "ribbonColors", new List<Color>());

            BFAuroraRibbonsLayer layer = new BFAuroraRibbonsLayer(config);
            layer.Init(parentGo.transform, 0);

            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Error && e.Message.Contains("no ribbon colors configured")));
        }

        [Test]
        public void Init_CreatesRootOnBackgroundLayer()
        {
            BFAuroraRibbonsLayerConfig config = CreateConfig();
            BFAuroraRibbonsLayer layer = new BFAuroraRibbonsLayer(config);

            layer.Init(parentGo.transform, 3);

            Assert.AreEqual(1, parentGo.transform.childCount);
            Transform root = parentGo.transform.GetChild(0);
            Assert.AreEqual(BFBackgroundStackManager.BackgroundLayer, root.gameObject.layer);
        }

        [Test]
        public void Init_SyncsRibbonCountToConfig()
        {
            BFAuroraRibbonsLayerConfig config = CreateConfig();
            SetField(config, "ribbonCount", 3);

            BFAuroraRibbonsLayer layer = new BFAuroraRibbonsLayer(config);
            layer.Init(parentGo.transform, 2);

            List<LineRenderer> renderers = GetField<List<LineRenderer>>(layer, "ribbonRenderers");
            Assert.AreEqual(3, renderers.Count);
            foreach (LineRenderer lr in renderers)
            {
                Assert.AreEqual(81, lr.positionCount);
                Assert.AreEqual(2, lr.sortingOrder);
            }
        }

        [Test]
        public void Tick_RibbonCountIncreased_GrowsRibbonsAndRenderers()
        {
            BFAuroraRibbonsLayerConfig config = CreateConfig();
            SetField(config, "ribbonCount", 1);
            BFAuroraRibbonsLayer layer = new BFAuroraRibbonsLayer(config);
            layer.Init(parentGo.transform, 0);

            SetField(config, "ribbonCount", 3);
            layer.Tick(0f);

            Assert.AreEqual(3, GetField<List<LineRenderer>>(layer, "ribbonRenderers").Count);
        }

        [Test]
        public void Tick_RibbonCountDecreased_ShrinksRibbonsAndRenderers()
        {
            BFAuroraRibbonsLayerConfig config = CreateConfig();
            SetField(config, "ribbonCount", 3);
            BFAuroraRibbonsLayer layer = new BFAuroraRibbonsLayer(config);
            layer.Init(parentGo.transform, 0);

            SetField(config, "ribbonCount", 1);
            layer.Tick(0f);

            Assert.AreEqual(1, GetField<List<LineRenderer>>(layer, "ribbonRenderers").Count);
        }

        [Test]
        public void Cleanup_ClearsInternalState()
        {
            BFAuroraRibbonsLayerConfig config = CreateConfig();
            BFAuroraRibbonsLayer layer = new BFAuroraRibbonsLayer(config);
            layer.Init(parentGo.transform, 0);

            layer.Cleanup();

            Assert.IsNull(GetField<Transform>(layer, "root"));
            Assert.IsNull(GetField<Material>(layer, "glowMaterial"));
            Assert.AreEqual(0, GetField<List<BFAuroraRibbon>>(layer, "ribbons").Count);
            Assert.AreEqual(0, GetField<List<LineRenderer>>(layer, "ribbonRenderers").Count);
        }
    }
}