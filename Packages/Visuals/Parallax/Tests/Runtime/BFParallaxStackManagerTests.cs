using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Visuals.Parallax;

namespace BFTools.Visuals.Parallax.Tests
{
    public class BFParallaxStackManagerTests
    {
        private class SpyParallaxLayer : IBFParallaxLayer
        {
            public int InitCount;
            public List<(Vector2 displacement, float dt)> TickCalls = new List<(Vector2, float)>();
            public int CleanupCount;

            public void Init(Transform parent, int sortingOrder) => InitCount++;
            public void Tick(Vector2 cameraDisplacement, float dt) => TickCalls.Add((cameraDisplacement, dt));
            public void Cleanup() => CleanupCount++;
        }

        private class SpyParallaxLayerConfig : BFParallaxLayerConfig
        {
            public readonly SpyParallaxLayer Layer = new SpyParallaxLayer();

            public override IBFParallaxLayer CreateLayer() => Layer;
        }

        private List<GameObject> createdGameObjects;
        private List<Object> createdAssets;

        [SetUp]
        public void SetUp()
        {
            createdGameObjects = new List<GameObject>();
            createdAssets = new List<Object>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject obj in createdGameObjects)
                if (obj != null)
                    Object.DestroyImmediate(obj);

            foreach (Object asset in createdAssets)
                if (asset != null)
                    Object.DestroyImmediate(asset);

            BFLoggerTestUtility.ResetState();
        }

        private static void SetField(object target, string fieldName, object value)
        {
            target.GetType()
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(target, value);
        }

        private static void InvokeUpdate(BFParallaxStackManager manager)
        {
            typeof(BFParallaxStackManager)
                .GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(manager, null);
        }

        private static void InvokeOnDisable(BFParallaxStackManager manager)
        {
            typeof(BFParallaxStackManager)
                .GetMethod("OnDisable", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(manager, null);
        }

        private SpyParallaxLayerConfig CreateSpyLayerConfig()
        {
            SpyParallaxLayerConfig config = ScriptableObject.CreateInstance<SpyParallaxLayerConfig>();
            createdAssets.Add(config);
            return config;
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

        private Camera CreateCamera()
        {
            GameObject cameraGo = new GameObject("Camera");
            createdGameObjects.Add(cameraGo);
            return cameraGo.AddComponent<Camera>();
        }

        private BFParallaxStackManager CreateManager(Camera camera, params BFParallaxStackConfig[] stackConfigs)
        {
            GameObject managerGo = new GameObject("ParallaxStackManager");
            createdGameObjects.Add(managerGo);
            managerGo.SetActive(false);

            BFParallaxStackManager manager = managerGo.AddComponent<BFParallaxStackManager>();
            SetField(manager, "stacks", new List<BFParallaxStackConfig>(stackConfigs));
            SetField(manager, "targetCameraOverride", camera);

            managerGo.SetActive(true);

            typeof(BFParallaxStackManager)
                .GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(manager, null);

            return manager;
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
        public void OnEnable_BuildsStacksSkippingNullSlots_AndLogsInfo()
        {
            SpyLoggerSink spy = InitializeLogging();
            SpyParallaxLayerConfig layerConfig = CreateSpyLayerConfig();
            BFParallaxStackConfig stackConfig = CreateStackConfig(layerConfig);
            Camera camera = CreateCamera();

            CreateManager(camera, stackConfig, null);

            Assert.AreEqual(1, layerConfig.Layer.InitCount);
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Warning && e.Message.Contains("skipping empty stack slot")));
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Info && e.Message.Contains("enabled with 1 stack(s), 1 layer(s) total")));
        }

        [Test]
        public void OnEnable_DuplicateInstance_LogsErrorAndDisablesSecondInstance()
        {
            Camera camera = CreateCamera();
            SpyParallaxLayerConfig layerConfig = CreateSpyLayerConfig();
            BFParallaxStackConfig stackConfig = CreateStackConfig(layerConfig);
            CreateManager(camera, stackConfig);

            SpyLoggerSink spy = InitializeLogging();
            BFParallaxStackManager second = CreateManager(camera, stackConfig);

            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Error && e.Message.Contains("another instance is already active")));
            Assert.IsFalse(second.enabled);
        }

        [Test]
        public void Update_TicksStacksWithCameraDisplacementSinceEnable()
        {
            Camera camera = CreateCamera();
            camera.transform.position = new Vector3(1f, 1f, 1f);
            SpyParallaxLayerConfig layerConfig = CreateSpyLayerConfig();
            BFParallaxStackConfig stackConfig = CreateStackConfig(layerConfig);
            BFParallaxStackManager manager = CreateManager(camera, stackConfig);

            camera.transform.position = new Vector3(4f, 5f, 1f);
            InvokeUpdate(manager);

            Assert.AreEqual(1, layerConfig.Layer.TickCalls.Count);
            Assert.AreEqual(new Vector2(3f, 4f), layerConfig.Layer.TickCalls[0].displacement);
        }

        [Test]
        public void OnDisable_CleansUpStacksAndClearsInstanceAllowingReEnable()
        {
            Camera camera = CreateCamera();
            SpyParallaxLayerConfig layerConfigA = CreateSpyLayerConfig();
            BFParallaxStackConfig stackConfigA = CreateStackConfig(layerConfigA);
            BFParallaxStackManager first = CreateManager(camera, stackConfigA);

            first.gameObject.SetActive(false);
            InvokeOnDisable(first);

            Assert.AreEqual(1, layerConfigA.Layer.CleanupCount);

            SpyParallaxLayerConfig layerConfigB = CreateSpyLayerConfig();
            BFParallaxStackConfig stackConfigB = CreateStackConfig(layerConfigB);
            SpyLoggerSink spy = InitializeLogging();
            CreateManager(camera, stackConfigB);

            Assert.AreEqual(1, layerConfigB.Layer.InitCount);
            Assert.IsFalse(spy.Entries.Exists(e => e.Level == LogLevel.Error));
        }
    }
}