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

        [SetUp]
        public void SetUp()
        {
            CleanupRoot();
        }

        [TearDown]
        public void TearDown()
        {
            CleanupRoot();
            BFLoggerTestUtility.ResetState();
            ResetInstantiateFunc();
        }

        private static readonly FieldInfo InstantiateFuncField =
            typeof(BFGlobalBootstrapper).GetField("instantiateFunc", BindingFlags.NonPublic | BindingFlags.Static);

        private static void ResetInstantiateFunc()
        {
            InstantiateFuncField.SetValue(null, (System.Func<GameObject, GameObject>)Object.Instantiate);
        }

        private static GameObject CreatePrefab(string name)
        {
            EnsureFolder(Root);

            GameObject go = new GameObject(name);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{Root}/{name}.prefab");
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void CleanupRoot()
        {
            if (AssetDatabase.IsValidFolder(Root))
            {
                AssetDatabase.DeleteAsset(Root);
                AssetDatabase.Refresh();
            }
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static void CreateConfigAsset(params GameObject[] prefabs)
        {
            EnsureFolder($"{Root}/Resources/{ConfigResourcePath.Substring(0, ConfigResourcePath.LastIndexOf('/'))}");

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
        public void Initialize_NullPrefabEntries_AreSkipped()
        {
            SpyLoggerSink spy = InitializeLogging();
            CreateConfigAsset(null, null);

            Assert.DoesNotThrow(InvokeInitialize);
            Assert.AreEqual(2, spy.Entries.FindAll(e => e.Level == LogLevel.Warning && e.Message.Contains("Skipped null prefab entry")).Count);
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Info && e.Message.Contains("Spawned 0 system prefab(s).")));
        }

        [Test]
        public void Initialize_NullSystemPrefabsArray_LogsErrorAndDoesNotThrow()
        {
            SpyLoggerSink spy = InitializeLogging();
            EnsureFolder($"{Root}/Resources/{ConfigResourcePath.Substring(0, ConfigResourcePath.LastIndexOf('/'))}");

            BFGlobalBootstrapperConfig config = ScriptableObject.CreateInstance<BFGlobalBootstrapperConfig>();
            typeof(BFGlobalBootstrapperConfig)
                .GetField("systemPrefabs", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(config, null);

            AssetDatabase.CreateAsset(config, $"{Root}/Resources/{ConfigResourcePath}.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Assert.DoesNotThrow(InvokeInitialize);
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Error && e.Message.Contains("null SystemPrefabs array")));
        }

        [Test]
        public void Initialize_PrefabInstantiationThrows_LogsErrorAndContinuesRemaining()
        {
            SpyLoggerSink spy = InitializeLogging();
            GameObject failingPrefab = CreatePrefab("FailingPrefab");
            GameObject goodPrefab = CreatePrefab("GoodPrefab");
            CreateConfigAsset(failingPrefab, goodPrefab);

            InstantiateFuncField.SetValue(null, (System.Func<GameObject, GameObject>)(prefab =>
            {
                if (prefab.name == "FailingPrefab")
                    throw new System.InvalidOperationException("simulated instantiation failure");

                return Object.Instantiate(prefab);
            }));

            Assert.DoesNotThrow(InvokeInitialize);

            GameObject instance = GameObject.Find("GoodPrefab(Clone)");
            if (instance != null)
                Object.DestroyImmediate(instance);

            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Error &&
                e.Message.Contains("Failed to instantiate system prefab 'FailingPrefab'") &&
                e.Message.Contains("simulated instantiation failure")));
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Info && e.Message.Contains("Spawned 1 system prefab(s).")));
        }
    }
}