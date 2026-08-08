using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BFTools.Core.Logger;

namespace BFTools.Core.Logger.Tests
{
    public class FileSinkTests
    {
        private const long MaxFileSizeBytes = 1 * 1024 * 1024;

        private string logDirectory;
        private string logFilePath;
        private string previousLogFilePath;

        [SetUp]
        public void SetUp()
        {
            logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
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
            if (Directory.Exists(logFilePath))
                Directory.Delete(logFilePath, true);
            else if (File.Exists(logFilePath))
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

        [Test]
        public void Write_ExistingFileBelowMaxSize_DoesNotRotate()
        {
            File.WriteAllText(logFilePath, "existing content" + System.Environment.NewLine, Encoding.UTF8);
            FileSink sink = new FileSink();

            sink.Write(LogLevel.Warning, new[] { "Tag" }, "new message", null, false);

            Assert.IsFalse(File.Exists(previousLogFilePath));
            string content = File.ReadAllText(logFilePath);
            StringAssert.Contains("existing content", content);
            StringAssert.Contains("new message", content);
        }

        [Test]
        public void Write_ExistingFileAtOrAboveMaxSize_RotatesToBackupBeforeAppending()
        {
            WriteFileOfSize(logFilePath, MaxFileSizeBytes, "old content");
            FileSink sink = new FileSink();

            sink.Write(LogLevel.Warning, new[] { "Tag" }, "new message", null, false);

            Assert.IsTrue(File.Exists(previousLogFilePath));
            StringAssert.Contains("old content", File.ReadAllText(previousLogFilePath));

            string newContent = File.ReadAllText(logFilePath);
            StringAssert.DoesNotContain("old content", newContent);
            StringAssert.Contains("new message", newContent);
        }

        [Test]
        public void Write_RotationWithExistingBackup_ReplacesOldBackup()
        {
            File.WriteAllText(previousLogFilePath, "stale backup content", Encoding.UTF8);
            WriteFileOfSize(logFilePath, MaxFileSizeBytes, "current content");
            FileSink sink = new FileSink();

            sink.Write(LogLevel.Warning, new[] { "Tag" }, "new message", null, false);

            string backupContent = File.ReadAllText(previousLogFilePath);
            StringAssert.DoesNotContain("stale backup content", backupContent);
            StringAssert.Contains("current content", backupContent);
        }

        private static void WriteFileOfSize(string path, long minimumSizeBytes, string marker)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(marker).Append(System.Environment.NewLine);
            builder.Append('a', (int)minimumSizeBytes);
            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        [Test]
        public void Constructor_LogDirectoryBlockedByFile_DoesNotThrowAndDisablesFileLogging()
        {
            if (Directory.Exists(logDirectory))
                Directory.Delete(logDirectory, true);
            File.WriteAllText(logDirectory, "blocking file");

            try
            {
                LogAssert.Expect(LogType.Warning, new Regex("Failed to create log directory"));

                FileSink sink = null;
                Assert.DoesNotThrow(() => sink = new FileSink());
                Assert.DoesNotThrow(() => sink.Write(LogLevel.Warning, new[] { "Tag" }, "message", null, false));

                Assert.IsFalse(File.Exists(logFilePath));
            }
            finally
            {
                File.Delete(logDirectory);
                Directory.CreateDirectory(logDirectory);
            }
        }

        [Test]
        public void Write_LogFilePathBlockedByDirectory_DoesNotThrowAndDisablesFurtherWrites()
        {
            FileSink sink = new FileSink();
            Directory.CreateDirectory(logFilePath);

            LogAssert.Expect(LogType.Warning, new Regex("Failed to write to log file"));
            Assert.DoesNotThrow(() => sink.Write(LogLevel.Warning, new[] { "Tag" }, "first", null, false));

            Assert.DoesNotThrow(() => sink.Write(LogLevel.Warning, new[] { "Tag" }, "second", null, false));
        }
    }
}