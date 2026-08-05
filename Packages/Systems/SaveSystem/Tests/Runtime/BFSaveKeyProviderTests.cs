using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Systems.SaveSystem;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Systems.SaveSystem.Tests
{
    public class BFSaveKeyProviderTests
    {
        [SetUp]
        public void SetUp()
        {
            ResetState();
        }

        [TearDown]
        public void TearDown()
        {
            ResetState();
            BFLoggerTestUtility.ResetState();
        }

        private static void ResetState()
        {
            typeof(BFSaveKeyProvider).GetField("cachedKey", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, null);
            typeof(BFSaveKeyProvider).GetField("cachedMacKey", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, null);

            string keyPath = BFSavePath.KeyFilePath;
            string tempPath = keyPath + ".tmp";

            if (File.Exists(keyPath))
                File.Delete(keyPath);

            if (File.Exists(tempPath))
                File.Delete(tempPath);
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
        public void GetKey_NoExistingKeyFile_GeneratesAndPersistsNewKeyAndLogsInfo()
        {
            SpyLoggerSink spy = InitializeLogging();

            byte[] key = BFSaveKeyProvider.GetKey();

            Assert.AreEqual(32, key.Length);
            Assert.IsTrue(File.Exists(BFSavePath.KeyFilePath));
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Info && e.Message.Contains("Generated new save key")));
        }

        [Test]
        public void GetKey_CalledTwice_ReturnsSameCachedInstance()
        {
            byte[] first = BFSaveKeyProvider.GetKey();
            byte[] second = BFSaveKeyProvider.GetKey();

            Assert.AreSame(first, second);
        }

        [Test]
        public void GetKey_ExistingKeyFileOnDisk_LoadsFromDiskAndLogsTrace()
        {
            byte[] existingKey = new byte[32];
            for (int i = 0; i < existingKey.Length; i++)
                existingKey[i] = (byte)i;
            File.WriteAllBytes(BFSavePath.KeyFilePath, existingKey);
            SpyLoggerSink spy = InitializeLogging();

            byte[] loadedKey = BFSaveKeyProvider.GetKey();

            CollectionAssert.AreEqual(existingKey, loadedKey);
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Trace && e.Message.Contains("Loaded existing save key")));
        }

        [Test]
        public void GetMacKey_CalledTwice_ReturnsSameCachedInstanceDerivedFromKey()
        {
            byte[] macFirst = BFSaveKeyProvider.GetMacKey();
            byte[] macSecond = BFSaveKeyProvider.GetMacKey();

            Assert.AreSame(macFirst, macSecond);
            Assert.AreEqual(32, macFirst.Length);
        }
    }
}