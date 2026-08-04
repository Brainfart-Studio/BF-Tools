using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Visuals.Parallax;

namespace BFTools.Visuals.Parallax.Tests
{
    public class BFParallaxStackTests
    {
        private class SpyParallaxLayer : IBFParallaxLayer
        {
            public int InitCount;
            public Transform InitParent;
            public int InitSortingOrder;
            public List<(Vector2 displacement, float dt)> TickCalls = new List<(Vector2, float)>();
            public int CleanupCount;

            public void Init(Transform parent, int sortingOrder)
            {
                InitCount++;
                InitParent = parent;
                InitSortingOrder = sortingOrder;
            }

            public void Tick(Vector2 cameraDisplacement, float dt)
            {
                TickCalls.Add((cameraDisplacement, dt));
            }

            public void Cleanup()
            {
                CleanupCount++;
            }
        }

        private class SpyParallaxLayerConfig : BFParallaxLayerConfig
        {
            public readonly SpyParallaxLayer Layer = new SpyParallaxLayer();

            public override IBFParallaxLayer CreateLayer() => Layer;
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

        private BFParallaxStackConfig CreateStackConfig(params BFParallaxLayerConfig[] layerConfigs)
        {
            BFParallaxStackConfig config = ScriptableObject.CreateInstance<BFParallaxStackConfig>();
            createdAssets.Add(config);

            typeof(BFParallaxStackConfig)
                .GetField("layers", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(config, new List<BFParallaxLayerConfig>(layerConfigs));

            return config;
        }

        private SpyParallaxLayerConfig CreateSpyLayerConfig()
        {
            SpyParallaxLayerConfig config = ScriptableObject.CreateInstance<SpyParallaxLayerConfig>();
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
            SpyParallaxLayerConfig configA = CreateSpyLayerConfig();
            SpyParallaxLayerConfig configB = CreateSpyLayerConfig();
            SpyParallaxLayerConfig configC = CreateSpyLayerConfig();
            BFParallaxStackConfig stackConfig = CreateStackConfig(configA, configB, configC);

            BFParallaxStack stack = new BFParallaxStack(stackConfig);
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
            SpyParallaxLayerConfig configA = CreateSpyLayerConfig();
            SpyParallaxLayerConfig configB = CreateSpyLayerConfig();
            BFParallaxStackConfig stackConfig = CreateStackConfig(configA, null, configB);

            BFParallaxStack stack = new BFParallaxStack(stackConfig);
            stack.Init(parentGo.transform, 0);

            Assert.AreEqual(2, stack.LayerCount);
            Assert.AreEqual(0, configA.Layer.InitSortingOrder);
            Assert.AreEqual(1, configB.Layer.InitSortingOrder);
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Warning && e.Message.Contains("skipping empty layer slot")));
        }

        [Test]
        public void Tick_DelegatesToAllLayersWithSameArguments()
        {
            SpyParallaxLayerConfig configA = CreateSpyLayerConfig();
            SpyParallaxLayerConfig configB = CreateSpyLayerConfig();
            BFParallaxStackConfig stackConfig = CreateStackConfig(configA, configB);
            BFParallaxStack stack = new BFParallaxStack(stackConfig);
            stack.Init(parentGo.transform, 0);

            stack.Tick(new Vector2(3f, 4f), 0.5f);

            Assert.AreEqual(1, configA.Layer.TickCalls.Count);
            Assert.AreEqual(new Vector2(3f, 4f), configA.Layer.TickCalls[0].displacement);
            Assert.AreEqual(0.5f, configA.Layer.TickCalls[0].dt);
            Assert.AreEqual(1, configB.Layer.TickCalls.Count);
        }

        [Test]
        public void Cleanup_CallsCleanupOnAllLayersAndClearsList()
        {
            SpyParallaxLayerConfig configA = CreateSpyLayerConfig();
            SpyParallaxLayerConfig configB = CreateSpyLayerConfig();
            BFParallaxStackConfig stackConfig = CreateStackConfig(configA, configB);
            BFParallaxStack stack = new BFParallaxStack(stackConfig);
            stack.Init(parentGo.transform, 0);

            stack.Cleanup();

            Assert.AreEqual(1, configA.Layer.CleanupCount);
            Assert.AreEqual(1, configB.Layer.CleanupCount);
            Assert.AreEqual(0, stack.LayerCount);
        }
    }
}