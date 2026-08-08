using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Systems.LevelBootstrapper.Tests
{
    public class BFLevelBootstrapperTests
    {
        private GameObject go;
        private List<Object> createdAssets;
        private List<GameObject> spawnedInstances;

        [SetUp]
        public void SetUp()
        {
            createdAssets = new List<Object>();
            spawnedInstances = new List<GameObject>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject instance in spawnedInstances)
                if (instance != null)
                    Object.DestroyImmediate(instance);

            if (go != null)
                Object.DestroyImmediate(go);

            foreach (Object asset in createdAssets)
                Object.DestroyImmediate(asset);

            BFLoggerTestUtility.ResetState();
        }

        private static BFLevelBootstrapConfig CreateConfig(params GameObject[] prefabs)
        {
            BFLevelBootstrapConfig config = ScriptableObject.CreateInstance<BFLevelBootstrapConfig>();
            typeof(BFLevelBootstrapConfig)
                .GetField("prefabsToInstantiate", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(config, prefabs);
            return config;
        }

        private BFLevelBootstrapper CreateBootstrapper(BFLevelBootstrapConfig config)
        {
            go = new GameObject("LevelBootstrapper");
            go.SetActive(false);

            BFLevelBootstrapper bootstrapper = go.AddComponent<BFLevelBootstrapper>();
            typeof(BFLevelBootstrapper)
                .GetField("config", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(bootstrapper, config);

            go.SetActive(true);

            typeof(BFLevelBootstrapper)
                .GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(bootstrapper, null);

            return bootstrapper;
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
        public void Awake_NoConfigAssigned_LogsError()
        {
            SpyLoggerSink spy = InitializeLogging();

            CreateBootstrapper(null);

            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Error && e.Message.Contains("No LevelBootstrapConfig assigned")));
        }

        [Test]
        public void Awake_WithPrefabs_InstantiatesNonNullPrefabsAndLogsCount()
        {
            SpyLoggerSink spy = InitializeLogging();
            GameObject prefabA = new GameObject("PrefabA");
            GameObject prefabB = new GameObject("PrefabB");
            createdAssets.Add(prefabA);
            createdAssets.Add(prefabB);
            BFLevelBootstrapConfig config = CreateConfig(prefabA, null, prefabB);
            createdAssets.Add(config);

            CreateBootstrapper(config);

            GameObject instanceA = GameObject.Find("PrefabA(Clone)");
            GameObject instanceB = GameObject.Find("PrefabB(Clone)");
            spawnedInstances.Add(instanceA);
            spawnedInstances.Add(instanceB);

            Assert.IsNotNull(instanceA);
            Assert.IsNotNull(instanceB);
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Info && e.Message.Contains("Spawned 2 prefab(s).")));
        }

        [Test]
        public void Awake_NullPrefabsToInstantiateArray_LogsErrorAndDoesNotThrow()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFLevelBootstrapConfig config = CreateConfig(null);
            createdAssets.Add(config);

            Assert.DoesNotThrow(() => CreateBootstrapper(config));
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Error && e.Message.Contains("null PrefabsToInstantiate array")));
        }
    }
}