using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;

namespace BFTools.Core.ConfigLookup.Tests
{
    public class BFConfigLookupBuilderTests
    {
        private struct TestEntry
        {
            public string key;
            public int value;
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
        public void Merge_NoDuplicateKeys_ReturnsAllEntriesKeyed()
        {
            InitializeLogging();
            List<TestEntry> entries = new List<TestEntry>
            {
                new TestEntry { key = "Hit", value = 1 },
                new TestEntry { key = "Explosion", value = 2 }
            };

            Dictionary<string, TestEntry> lookup = BFConfigLookupBuilder.Merge(entries, e => e.key, "Test", "Owner", "eventName");

            Assert.AreEqual(2, lookup.Count);
            Assert.AreEqual(1, lookup["Hit"].value);
            Assert.AreEqual(2, lookup["Explosion"].value);
        }

        [Test]
        public void Merge_DuplicateKey_LastEntryWinsAndLogsWarning()
        {
            SpyLoggerSink spy = InitializeLogging();
            List<TestEntry> entries = new List<TestEntry>
            {
                new TestEntry { key = "Hit", value = 1 },
                new TestEntry { key = "Hit", value = 2 }
            };

            Dictionary<string, TestEntry> lookup = BFConfigLookupBuilder.Merge(entries, e => e.key, "Test", "Owner", "eventName");

            Assert.AreEqual(1, lookup.Count);
            Assert.AreEqual(2, lookup["Hit"].value);
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Warning && e.Message.Contains("Duplicate eventName 'Hit'")));
        }

        [Test]
        public void Merge_CustomKeyLabel_UsesLabelInWarningMessage()
        {
            SpyLoggerSink spy = InitializeLogging();
            List<TestEntry> entries = new List<TestEntry>
            {
                new TestEntry { key = "A", value = 1 },
                new TestEntry { key = "A", value = 2 }
            };

            BFConfigLookupBuilder.Merge(entries, e => e.key, "Test", "Owner", keyLabel: "id");

            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Warning && e.Message.Contains("Duplicate id 'A'")));
        }

        [Test]
        public void Merge_EmptyEntries_ReturnsEmptyLookupAndLogsDebugCount()
        {
            SpyLoggerSink spy = InitializeLogging();

            Dictionary<string, TestEntry> lookup = BFConfigLookupBuilder.Merge(new List<TestEntry>(), e => e.key, "Test", "Owner", "eventName");

            Assert.AreEqual(0, lookup.Count);
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Debug && e.Message.Contains("Built lookup with 0 entrie(s) on 'Owner'")));
        }
    }
}