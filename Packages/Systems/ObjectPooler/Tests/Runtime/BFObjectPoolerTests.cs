using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Core.ServiceLocator;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Systems.ObjectPooler.Tests
{
    public class BFObjectPoolerTests
    {
        private GameObject go;
        private List<Object> createdAssets;

        [SetUp]
        public void SetUp()
        {
            createdAssets = new List<Object>();
        }

        [TearDown]
        public void TearDown()
        {
            if (go != null)
                Object.DestroyImmediate(go);

            foreach (Object asset in createdAssets)
                Object.DestroyImmediate(asset);

            BFServiceLocator.Unregister<BFObjectPooler>();
            BFLoggerTestUtility.ResetState();
        }

        private static BFObjectPoolConfig CreateConfig(params (string key, GameObject prefab, int prewarmCount)[] entries)
        {
            BFObjectPoolConfig config = ScriptableObject.CreateInstance<BFObjectPoolConfig>();
            List<BFObjectPoolConfig.PoolEntry> list = new List<BFObjectPoolConfig.PoolEntry>();
            foreach (var e in entries)
                list.Add(new BFObjectPoolConfig.PoolEntry { key = e.key, prefab = e.prefab, prewarmCount = e.prewarmCount });

            typeof(BFObjectPoolConfig)
                .GetField("poolEntries", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(config, list);

            return config;
        }

        private BFObjectPooler CreatePooler(BFObjectPoolConfig config)
        {
            go = new GameObject("ObjectPooler");
            go.SetActive(false);

            BFObjectPooler pooler = go.AddComponent<BFObjectPooler>();
            typeof(BFObjectPooler)
                .GetField("config", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(pooler, config);

            go.SetActive(true);
            return pooler;
        }

        private static int GetPoolCount(BFObjectPooler pooler, string key)
        {
            var pools = (Dictionary<string, Queue<GameObject>>)typeof(BFObjectPooler)
                .GetField("pools", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(pooler);
            return pools[key].Count;
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
        public void Awake_PrewarmsConfiguredEntries_AndRegistersWithServiceLocator()
        {
            GameObject bulletPrefab = new GameObject("Bullet");
            createdAssets.Add(bulletPrefab);
            BFObjectPoolConfig config = CreateConfig(("Bullet", bulletPrefab, 2));
            createdAssets.Add(config);

            BFObjectPooler pooler = CreatePooler(config);

            Assert.AreSame(pooler, BFServiceLocator.Get<BFObjectPooler>());
            Assert.AreEqual(2, GetPoolCount(pooler, "Bullet"));
            Assert.AreEqual(2, go.transform.childCount);
            Assert.IsFalse(go.transform.GetChild(0).gameObject.activeSelf);
        }

        [Test]
        public void Get_PoolHasInstance_DequeuesWithoutWarningAndActivatesInstance()
        {
            GameObject bulletPrefab = new GameObject("Bullet");
            createdAssets.Add(bulletPrefab);
            BFObjectPoolConfig config = CreateConfig(("Bullet", bulletPrefab, 1));
            createdAssets.Add(config);
            BFObjectPooler pooler = CreatePooler(config);
            SpyLoggerSink spy = InitializeLogging();

            GameObject instance = pooler.Get("Bullet");

            Assert.IsTrue(instance.activeSelf);
            Assert.AreEqual(0, GetPoolCount(pooler, "Bullet"));
            Assert.IsFalse(spy.Entries.Exists(e => e.Level == LogLevel.Warning));
        }

        [Test]
        public void Get_PoolExhausted_LogsWarningAndInstantiatesNewInstance()
        {
            GameObject bulletPrefab = new GameObject("Bullet");
            createdAssets.Add(bulletPrefab);
            BFObjectPoolConfig config = CreateConfig(("Bullet", bulletPrefab, 0));
            createdAssets.Add(config);
            BFObjectPooler pooler = CreatePooler(config);
            SpyLoggerSink spy = InitializeLogging();

            GameObject instance = pooler.Get("Bullet");

            Assert.IsNotNull(instance);
            Assert.IsTrue(instance.activeSelf);
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Warning && e.Message.Contains("Pool 'Bullet' exhausted")));
        }

        [Test]
        public void Release_ReturnsInstanceToPoolInactiveForReuse()
        {
            GameObject bulletPrefab = new GameObject("Bullet");
            createdAssets.Add(bulletPrefab);
            BFObjectPoolConfig config = CreateConfig(("Bullet", bulletPrefab, 1));
            createdAssets.Add(config);
            BFObjectPooler pooler = CreatePooler(config);
            GameObject instance = pooler.Get("Bullet");

            pooler.Release(instance);

            Assert.IsFalse(instance.activeSelf);
            Assert.AreEqual(1, GetPoolCount(pooler, "Bullet"));
            Assert.AreSame(instance, pooler.Get("Bullet"));
        }

        [Test]
        public void OnDestroy_UnregistersFromServiceLocator()
        {
            GameObject bulletPrefab = new GameObject("Bullet");
            createdAssets.Add(bulletPrefab);
            BFObjectPoolConfig config = CreateConfig(("Bullet", bulletPrefab, 0));
            createdAssets.Add(config);
            CreatePooler(config);

            Object.DestroyImmediate(go);
            go = null;

            Assert.Throws<KeyNotFoundException>(() => BFServiceLocator.Get<BFObjectPooler>());
        }
    }
}