using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Systems.GlobalBootstrapper.Tests
{
    public class BFGlobalBootstrapperTests
    {
        private const string Root = "Assets/_BFGlobalBootstrapperTests";
        private const string ConfigResourcePath = "BFTools/GlobalBootstrapConfig";

        private List<GameObject> spawnedInstances;

        [SetUp]
        public void SetUp()
        {
            spawnedInstances = new List<GameObject>();
            CleanupRoot();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject instance in spawnedInstances)
                if (instance != null)
                    Object.DestroyImmediate(instance);

            CleanupRoot();
            BFLoggerTestUtility.ResetState();
        }

        private static void CleanupRoot()
        {
            if (AssetDatabase.IsValidFolder(Root))
            {
                AssetDatabase.DeleteAsset(Root);
                AssetDatabase.Refresh();
            }
        }

        private static GameObject CreatePrefab(string name)
        {
            GameObject go = new GameObject(name);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{Root}/{name}.prefab");
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void CreateConfigAsset(params GameObject[] prefabs)
        {
            BFGlobalBootstrapperConfig config = ScriptableObject.CreateInstance<BFGlobalBootstrapperConfig>();
            typeof(BFGlobalBootstrapperConfig)
                .GetField("systemPrefabs", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(config, prefabs);

            AssetDatabase.CreateAsset(config, $"{Root}/Resources/{ConfigResourcePath}.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void InvokeInitialize()
        {
            typeof(BFGlobalBootstrapper)
                .GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, null);
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
        public void Initialize_NoConfigFound_LogsError()
        {
            SpyLoggerSink spy = InitializeLogging();

            InvokeInitialize();

            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Error && e.Message.Contains("No GlobalBootstrapConfig found")));
        }

        [Test]
        public void Initialize_WithPrefabs_InstantiatesNonNullPrefabsAndLogsCount()
        {
            SpyLoggerSink spy = InitializeLogging();
            GameObject prefabA = CreatePrefab("PrefabA");
            GameObject prefabB = CreatePrefab("PrefabB");
            CreateConfigAsset(prefabA, null, prefabB);

            InvokeInitialize();

            GameObject instanceA = GameObject.Find("PrefabA(Clone)");
            GameObject instanceB = GameObject.Find("PrefabB(Clone)");
            spawnedInstances.Add(instanceA);
            spawnedInstances.Add(instanceB);

            Assert.IsNotNull(instanceA);
            Assert.IsNotNull(instanceB);
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Info && e.Message.Contains("Spawned 2 system prefab(s).")));
        }

        [Test]
        public void Initialize_NullPrefabEntries_AreSkipped()
        {
            InitializeLogging();
            CreateConfigAsset(null, null);

            Assert.DoesNotThrow(InvokeInitialize);
        }
    }
}