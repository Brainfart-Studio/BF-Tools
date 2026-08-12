using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Systems.SaveSystem;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Systems.SaveSystem.Tests
{
    public class BFSaveVersionMigratorTests
    {
        private class FakeSaveData
        {
        }

        [TearDown]
        public void TearDown()
        {
            BFLoggerTestUtility.ResetState();
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
        public void Migrate_FromCurrentVersion_ReturnsSameInstanceAndLogsNoMigrationNeeded()
        {
            SpyLoggerSink spy = InitializeLogging();
            FakeSaveData data = new FakeSaveData();

            object result = BFSaveVersionMigrator.Migrate(data, BFSaveVersionMigrator.CurrentVersion);

            Assert.AreSame(data, result);
            Assert.IsTrue(spy.Entries.Exists(e => e.Message.Contains("No migration needed")));
        }

        [Test]
        public void Migrate_FromOlderVersion_ReturnsSameInstanceAndLogsNoStepsImplemented()
        {
            SpyLoggerSink spy = InitializeLogging();
            FakeSaveData data = new FakeSaveData();

            object result = BFSaveVersionMigrator.Migrate(data, BFSaveVersionMigrator.CurrentVersion - 1);

            Assert.AreSame(data, result);
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Warning && e.Message.Contains("No migration steps implemented")));
        }
    }
}