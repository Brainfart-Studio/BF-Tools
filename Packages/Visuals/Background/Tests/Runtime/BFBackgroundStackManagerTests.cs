using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Visuals.Background;

namespace BFTools.Visuals.Background.Tests
{
    public class BFBackgroundStackManagerTests
    {
        private class SpyBackgroundLayer : IBFBackgroundLayer
        {
            public int InitCount;
            public List<float> TickCalls = new List<float>();
            public int CleanupCount;

            public void Init(Transform parent, int sortingOrder) => InitCount++;
            public void Tick(float dt) => TickCalls.Add(dt);
            public void Cleanup() => CleanupCount++;
        }

        private class SpyBackgroundLayerConfig : BFBackgroundLayerConfig
        {
            public readonly SpyBackgroundLayer Layer = new SpyBackgroundLayer();

            public override IBFBackgroundLayer CreateLayer() => Layer;
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

        private static void InvokeUpdate(BFBackgroundStackManager manager)
        {
            typeof(BFBackgroundStackManager)
                .GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(manager, null);
        }

        // BFBackgroundStackCamera (internal) creates a real background Camera GameObject on
        // Init that Cleanup() only destroys via Object.Destroy, which no-ops in EditMode - so
        // tests must reach it via reflection and destroy it directly, or it leaks into the scene.
        private static GameObject GetBackgroundCameraGameObject(BFBackgroundStackManager manager)
        {
            object stackCamera = typeof(BFBackgroundStackManager)
                .GetField("stackCamera", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(manager);
            if (stackCamera == null)
                return null;

            Camera backgroundCamera = (Camera)stackCamera.GetType()
                .GetField("backgroundCamera", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(stackCamera);
            return backgroundCamera != null ? backgroundCamera.gameObject : null;
        }

        private SpyBackgroundLayerConfig CreateSpyLayerConfig()
        {
            SpyBackgroundLayerConfig config = ScriptableObject.CreateInstance<SpyBackgroundLayerConfig>();
            createdAssets.Add(config);
            return config;
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

        private Camera CreateCamera()
        {
            GameObject cameraGo = new GameObject("Camera");
            createdGameObjects.Add(cameraGo);
            return cameraGo.AddComponent<Camera>();
        }

        private BFBackgroundStackManager CreateManager(Camera camera, params BFBackgroundStackConfig[] stackConfigs)
        {
            GameObject managerGo = new GameObject("BackgroundStackManager");
            createdGameObjects.Add(managerGo);
            managerGo.SetActive(false);

            BFBackgroundStackManager manager = managerGo.AddComponent<BFBackgroundStackManager>();
            SetField(manager, "stacks", new List<BFBackgroundStackConfig>(stackConfigs));
            SetField(manager, "targetCameraOverride", camera);

            managerGo.SetActive(true);

            GameObject leakedCamera = GetBackgroundCameraGameObject(manager);
            if (leakedCamera != null)
                createdGameObjects.Add(leakedCamera);

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
            SpyBackgroundLayerConfig layerConfig = CreateSpyLayerConfig();
            BFBackgroundStackConfig stackConfig = CreateStackConfig(layerConfig);
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
            SpyBackgroundLayerConfig layerConfig = CreateSpyLayerConfig();
            BFBackgroundStackConfig stackConfig = CreateStackConfig(layerConfig);
            CreateManager(camera, stackConfig);

            SpyLoggerSink spy = InitializeLogging();
            BFBackgroundStackManager second = CreateManager(camera, stackConfig);

            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Error && e.Message.Contains("another instance is already active")));
            Assert.IsFalse(second.enabled);
        }

        [Test]
        public void Update_TicksAllActiveStacksOncePerCall()
        {
            Camera camera = CreateCamera();
            SpyBackgroundLayerConfig layerConfig = CreateSpyLayerConfig();
            BFBackgroundStackConfig stackConfig = CreateStackConfig(layerConfig);
            BFBackgroundStackManager manager = CreateManager(camera, stackConfig);

            InvokeUpdate(manager);
            InvokeUpdate(manager);

            Assert.AreEqual(2, layerConfig.Layer.TickCalls.Count);
        }

        [Test]
        public void OnDisable_CleansUpStacksAndClearsInstanceAllowingReEnable()
        {
            Camera camera = CreateCamera();
            SpyBackgroundLayerConfig layerConfigA = CreateSpyLayerConfig();
            BFBackgroundStackConfig stackConfigA = CreateStackConfig(layerConfigA);
            BFBackgroundStackManager first = CreateManager(camera, stackConfigA);

            first.gameObject.SetActive(false);

            Assert.AreEqual(1, layerConfigA.Layer.CleanupCount);

            SpyBackgroundLayerConfig layerConfigB = CreateSpyLayerConfig();
            BFBackgroundStackConfig stackConfigB = CreateStackConfig(layerConfigB);
            SpyLoggerSink spy = InitializeLogging();
            CreateManager(camera, stackConfigB);

            Assert.AreEqual(1, layerConfigB.Layer.InitCount);
            Assert.IsFalse(spy.Entries.Exists(e => e.Level == LogLevel.Error));
        }
    }
}