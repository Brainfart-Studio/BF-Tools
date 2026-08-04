using System.IO;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.Logger;

namespace BFTools.Core.Logger.Tests
{
    public class FileSinkTests
    {
        private string logFilePath;
        private string previousLogFilePath;

        [SetUp]
        public void SetUp()
        {
            string logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
            logFilePath = Path.Combine(logDirectory, "bftools.log");
            previousLogFilePath = Path.Combine(logDirectory, "bftools.log.bak");
            DeleteLogFiles();
        }

        [TearDown]
        public void TearDown()
        {
            DeleteLogFiles();
        }

        private void DeleteLogFiles()
        {
            if (File.Exists(logFilePath))
                File.Delete(logFilePath);
            if (File.Exists(previousLogFilePath))
                File.Delete(previousLogFilePath);
        }

        [TestCase(LogLevel.Trace)]
        [TestCase(LogLevel.Debug)]
        [TestCase(LogLevel.Info)]
        public void Write_BelowWarning_DoesNotCreateFile(LogLevel level)
        {
            FileSink sink = new FileSink();

            sink.Write(level, new[] { "Tag" }, "message", null, false);

            Assert.IsFalse(File.Exists(logFilePath));
        }

        [TestCase(LogLevel.Warning)]
        [TestCase(LogLevel.Error)]
        [TestCase(LogLevel.Critical)]
        public void Write_WarningOrAbove_AppendsLineContainingTagAndMessage(LogLevel level)
        {
            FileSink sink = new FileSink();

            sink.Write(level, new[] { "Gameplay" }, "hello", null, false);

            Assert.IsTrue(File.Exists(logFilePath));
            string content = File.ReadAllText(logFilePath);
            StringAssert.Contains($"[{level}]", content);
            StringAssert.Contains("[Gameplay]", content);
            StringAssert.Contains("hello", content);
        }

        [Test]
        public void Write_CalledTwice_AppendsBothLines()
        {
            FileSink sink = new FileSink();

            sink.Write(LogLevel.Warning, new[] { "Tag" }, "first", null, false);
            sink.Write(LogLevel.Warning, new[] { "Tag" }, "second", null, false);

            string[] lines = File.ReadAllLines(logFilePath);
            Assert.AreEqual(2, lines.Length);
            StringAssert.Contains("first", lines[0]);
            StringAssert.Contains("second", lines[1]);
        }

        [Test]
        public void Write_IncludeStackTraceFalse_WritesSingleLine()
        {
            FileSink sink = new FileSink();

            sink.Write(LogLevel.Warning, new[] { "Tag" }, "message", null, false);

            string[] lines = File.ReadAllLines(logFilePath);
            Assert.AreEqual(1, lines.Length);
        }

        [Test]
        public void Write_IncludeStackTraceTrue_WritesAdditionalLines()
        {
            FileSink sink = new FileSink();

            sink.Write(LogLevel.Warning, new[] { "Tag" }, "message", null, true);

            string[] lines = File.ReadAllLines(logFilePath);
            Assert.Greater(lines.Length, 1);
        }
    }
}