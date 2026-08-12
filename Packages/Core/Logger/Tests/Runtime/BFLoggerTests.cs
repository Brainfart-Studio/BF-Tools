using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;

namespace BFTools.Core.Logger.Tests
{
    public class BFLoggerTests
    {
        [TearDown]
        public void TearDown()
        {
            BFLoggerTestUtility.ResetState();
        }

        private static BFLoggerConfig CreateConfig(
            LogLevel globalMinimumLevel = LogLevel.Info,
            bool useTagAllowlist = false,
            LogLevel stackTraceMinimumLevel = LogLevel.Error)
        {
            BFLoggerConfig config = ScriptableObject.CreateInstance<BFLoggerConfig>();
            SerializedObject serializedConfig = new SerializedObject(config);
            serializedConfig.FindProperty("globalMinimumLevel").enumValueIndex = (int)globalMinimumLevel;
            serializedConfig.FindProperty("useTagAllowlist").boolValue = useTagAllowlist;
            serializedConfig.FindProperty("stackTraceMinimumLevel").enumValueIndex = (int)stackTraceMinimumLevel;
            serializedConfig.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }

        private static void AddTagOverride(BFLoggerConfig config, string tag, LogLevel minimumLevel)
        {
            SerializedObject serializedConfig = new SerializedObject(config);
            SerializedProperty overrides = serializedConfig.FindProperty("tagLevelOverrides");
            int index = overrides.arraySize;
            overrides.InsertArrayElementAtIndex(index);
            SerializedProperty element = overrides.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("tag").stringValue = tag;
            element.FindPropertyRelative("minimumLevel").enumValueIndex = (int)minimumLevel;
            serializedConfig.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddAllowedTag(BFLoggerConfig config, string tag)
        {
            SerializedObject serializedConfig = new SerializedObject(config);
            SerializedProperty allowlist = serializedConfig.FindProperty("tagAllowlist");
            int index = allowlist.arraySize;
            allowlist.InsertArrayElementAtIndex(index);
            allowlist.GetArrayElementAtIndex(index).stringValue = tag;
            serializedConfig.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void LogAtLevel(LogLevel level, string tag, string message)
        {
            switch (level)
            {
                case LogLevel.Trace:
                    BFLogger.Trace(tag, message);
                    break;
                case LogLevel.Debug:
                    BFLogger.Debug(tag, message);
                    break;
                case LogLevel.Info:
                    BFLogger.Info(tag, message);
                    break;
                case LogLevel.Warning:
                    BFLogger.Warning(tag, message);
                    break;
                case LogLevel.Error:
                    BFLogger.Error(tag, message);
                    break;
                case LogLevel.Critical:
                    BFLogger.Critical(tag, message);
                    break;
            }
        }

        [Test]
        public void Info_WritesEntryWithCorrectLevelTagAndMessage()
        {
            BFLoggerConfig config = CreateConfig();
            SpyLoggerSink spy = new SpyLoggerSink();
            BFLogger.Initialize(config, spy);

            BFLogger.Info("Gameplay", "hello");

            Assert.AreEqual(1, spy.Entries.Count);
            Assert.AreEqual(LogLevel.Info, spy.Entries[0].Level);
            Assert.AreEqual("hello", spy.Entries[0].Message);
            CollectionAssert.AreEqual(new[] { "Gameplay" }, spy.Entries[0].Tags);
        }

        [TestCase(LogLevel.Trace, false)]
        [TestCase(LogLevel.Debug, false)]
        [TestCase(LogLevel.Info, true)]
        [TestCase(LogLevel.Warning, true)]
        [TestCase(LogLevel.Error, true)]
        [TestCase(LogLevel.Critical, true)]
        public void Log_RespectsGlobalMinimumLevel(LogLevel level, bool expectedLogged)
        {
            BFLoggerConfig config = CreateConfig(globalMinimumLevel: LogLevel.Info);
            SpyLoggerSink spy = new SpyLoggerSink();
            BFLogger.Initialize(config, spy);

            LogAtLevel(level, "Tag", "message");

            Assert.AreEqual(expectedLogged, spy.Entries.Count == 1);
        }

        [Test]
        public void TagOverride_LowersMinimumLevelForMatchingTag()
        {
            BFLoggerConfig config = CreateConfig(globalMinimumLevel: LogLevel.Warning);
            AddTagOverride(config, "Verbose", LogLevel.Trace);
            SpyLoggerSink spy = new SpyLoggerSink();
            BFLogger.Initialize(config, spy);

            BFLogger.Debug("Verbose", "shown");
            BFLogger.Debug("Other", "hidden");

            Assert.AreEqual(1, spy.Entries.Count);
            Assert.AreEqual("shown", spy.Entries[0].Message);
        }

        [Test]
        public void TagOverride_CanRaiseMinimumLevelForMatchingTag()
        {
            BFLoggerConfig config = CreateConfig(globalMinimumLevel: LogLevel.Trace);
            AddTagOverride(config, "Noisy", LogLevel.Error);
            SpyLoggerSink spy = new SpyLoggerSink();
            BFLogger.Initialize(config, spy);

            BFLogger.Info("Noisy", "hidden");
            BFLogger.Info("Other", "shown");

            Assert.AreEqual(1, spy.Entries.Count);
            Assert.AreEqual("shown", spy.Entries[0].Message);
        }

        [Test]
        public void Allowlist_OnlyLogsAllowedTags()
        {
            BFLoggerConfig config = CreateConfig(useTagAllowlist: true);
            AddAllowedTag(config, "Allowed");
            SpyLoggerSink spy = new SpyLoggerSink();
            BFLogger.Initialize(config, spy);

            BFLogger.Info("Allowed", "shown");
            BFLogger.Info("Blocked", "hidden");

            Assert.AreEqual(1, spy.Entries.Count);
            Assert.AreEqual("shown", spy.Entries[0].Message);
        }

        [Test]
        public void MultipleSinks_AllReceiveTheSameLogCall()
        {
            BFLoggerConfig config = CreateConfig();
            SpyLoggerSink spyA = new SpyLoggerSink();
            SpyLoggerSink spyB = new SpyLoggerSink();
            BFLogger.Initialize(config, spyA, spyB);

            BFLogger.Info("Tag", "message");

            Assert.AreEqual(1, spyA.Entries.Count);
            Assert.AreEqual(1, spyB.Entries.Count);
        }

        [Test]
        public void Initialize_ClearsSinksFromPreviousInitialize()
        {
            BFLoggerConfig config = CreateConfig();
            SpyLoggerSink oldSpy = new SpyLoggerSink();
            SpyLoggerSink newSpy = new SpyLoggerSink();
            BFLogger.Initialize(config, oldSpy);

            BFLogger.Initialize(config, newSpy);
            BFLogger.Info("Tag", "message");

            Assert.AreEqual(0, oldSpy.Entries.Count);
            Assert.AreEqual(1, newSpy.Entries.Count);
        }

        [Test]
        public void IncludeStackTrace_IsFalseBelowStackTraceMinimumLevel()
        {
            BFLoggerConfig config = CreateConfig(stackTraceMinimumLevel: LogLevel.Error);
            SpyLoggerSink spy = new SpyLoggerSink();
            BFLogger.Initialize(config, spy);

            BFLogger.Warning("Tag", "message");

            Assert.IsFalse(spy.Entries[0].IncludeStackTrace);
        }

        [Test]
        public void IncludeStackTrace_IsTrueAtOrAboveStackTraceMinimumLevel()
        {
            BFLoggerConfig config = CreateConfig(stackTraceMinimumLevel: LogLevel.Error);
            SpyLoggerSink spy = new SpyLoggerSink();
            BFLogger.Initialize(config, spy);

            BFLogger.Error("Tag", "message");

            Assert.IsTrue(spy.Entries[0].IncludeStackTrace);
        }

        [Test]
        public void Initialize_NoSinks_FallsBackToConsoleSinkAndLogsWarning()
        {
            BFLoggerConfig config = CreateConfig();

            LogAssert.Expect(LogType.Warning, new Regex("Initialize was called with no sinks"));
            BFLogger.Initialize(config);

            LogAssert.Expect(LogType.Log, new Regex("hello"));
            BFLogger.Info("Tag", "hello");
        }

        [Test]
        public void Initialize_NullSinksArray_FallsBackToConsoleSinkAndLogsWarning()
        {
            BFLoggerConfig config = CreateConfig();

            LogAssert.Expect(LogType.Warning, new Regex("Initialize was called with no sinks"));
            BFLogger.Initialize(config, null);

            LogAssert.Expect(LogType.Log, new Regex("hello"));
            BFLogger.Info("Tag", "hello");
        }
    }
}