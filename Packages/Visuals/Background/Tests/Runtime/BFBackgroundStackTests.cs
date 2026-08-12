using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Visuals.Background;

namespace BFTools.Visuals.Background.Tests
{
    public class BFBackgroundStackTests
    {
        private class SpyBackgroundLayer : IBFBackgroundLayer
        {
            public int InitCount;
            public Transform InitParent;
            public int InitSortingOrder;
            public List<float> TickCalls = new List<float>();
            public int CleanupCount;

            public void Init(Transform parent, int sortingOrder)
            {
                InitCount++;
                InitParent = parent;
                InitSortingOrder = sortingOrder;
            }

            public void Tick(float dt)
            {
                TickCalls.Add(dt);
            }

            public void Cleanup()
            {
                CleanupCount++;
            }
        }

        private class SpyBackgroundLayerConfig : BFBackgroundLayerConfig
        {
            public readonly SpyBackgroundLayer Layer = new SpyBackgroundLayer();

            public override IBFBackgroundLayer CreateLayer() => Layer;
        }

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

        private BFBackgroundStackConfig CreateStackConfig(params BFBackgroundLayerConfig[] layerConfigs)
        {
            BFBackgroundStackConfig config = ScriptableObject.CreateInstance<BFBackgroundStackConfig>();
            createdAssets.Add(config);

            typeof(BFBackgroundStackConfig)
                .GetField("layers", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(config, new List<BFBackgroundLayerConfig>(layerConfigs));

            return config;
        }

        private SpyBackgroundLayerConfig CreateSpyLayerConfig()
        {
            SpyBackgroundLayerConfig config = ScriptableObject.CreateInstance<SpyBackgroundLayerConfig>();
            createdAssets.Add(config);
            return config;
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
        public void Init_CreatesLayersInOrderWithIncrementingSortingOrder()
        {
            SpyLoggerSink spy = InitializeLogging();
            SpyBackgroundLayerConfig configA = CreateSpyLayerConfig();
            SpyBackgroundLayerConfig configB = CreateSpyLayerConfig();
            SpyBackgroundLayerConfig configC = CreateSpyLayerConfig();
            BFBackgroundStackConfig stackConfig = CreateStackConfig(configA, configB, configC);

            BFBackgroundStack stack = new BFBackgroundStack(stackConfig);
            stack.Init(parentGo.transform, 10);

            Assert.AreEqual(3, stack.LayerCount);
            Assert.AreEqual(1, configA.Layer.InitCount);
            Assert.AreEqual(10, configA.Layer.InitSortingOrder);
            Assert.AreEqual(11, configB.Layer.InitSortingOrder);
            Assert.AreEqual(12, configC.Layer.InitSortingOrder);
            Assert.AreEqual(parentGo.transform, configA.Layer.InitParent);
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Info && e.Message.Contains("initialized with 3 layer(s)")));
        }

        [Test]
        public void Init_NullLayerSlot_SkipsAndLogsWarning()
        {
            SpyLoggerSink spy = InitializeLogging();
            SpyBackgroundLayerConfig configA = CreateSpyLayerConfig();
            SpyBackgroundLayerConfig configB = CreateSpyLayerConfig();
            BFBackgroundStackConfig stackConfig = CreateStackConfig(configA, null, configB);

            BFBackgroundStack stack = new BFBackgroundStack(stackConfig);
            stack.Init(parentGo.transform, 0);

            Assert.AreEqual(2, stack.LayerCount);
            Assert.AreEqual(0, configA.Layer.InitSortingOrder);
            Assert.AreEqual(1, configB.Layer.InitSortingOrder);
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Warning && e.Message.Contains("skipping empty layer slot")));
        }

        [Test]
        public void Tick_DelegatesToAllLayersWithSameArgument()
        {
            SpyBackgroundLayerConfig configA = CreateSpyLayerConfig();
            SpyBackgroundLayerConfig configB = CreateSpyLayerConfig();
            BFBackgroundStackConfig stackConfig = CreateStackConfig(configA, configB);
            BFBackgroundStack stack = new BFBackgroundStack(stackConfig);
            stack.Init(parentGo.transform, 0);

            stack.Tick(0.5f);

            Assert.AreEqual(1, configA.Layer.TickCalls.Count);
            Assert.AreEqual(0.5f, configA.Layer.TickCalls[0]);
            Assert.AreEqual(1, configB.Layer.TickCalls.Count);
        }

        [Test]
        public void Cleanup_CallsCleanupOnAllLayersAndClearsList()
        {
            SpyBackgroundLayerConfig configA = CreateSpyLayerConfig();
            SpyBackgroundLayerConfig configB = CreateSpyLayerConfig();
            BFBackgroundStackConfig stackConfig = CreateStackConfig(configA, configB);
            BFBackgroundStack stack = new BFBackgroundStack(stackConfig);
            stack.Init(parentGo.transform, 0);

            stack.Cleanup();

            Assert.AreEqual(1, configA.Layer.CleanupCount);
            Assert.AreEqual(1, configB.Layer.CleanupCount);
            Assert.AreEqual(0, stack.LayerCount);
        }
    }
}