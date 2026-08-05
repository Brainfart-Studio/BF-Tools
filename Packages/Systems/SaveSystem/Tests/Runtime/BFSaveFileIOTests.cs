using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Systems.SaveSystem;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Systems.SaveSystem.Tests
{
    public class BFSaveFileIOTests
    {
        private string scratchDir;

        [SetUp]
        public void SetUp()
        {
            scratchDir = Path.Combine(Path.GetTempPath(), "BFSaveFileIOTests");
            if (Directory.Exists(scratchDir))
                Directory.Delete(scratchDir, true);
            Directory.CreateDirectory(scratchDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(scratchDir))
                Directory.Delete(scratchDir, true);

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
        public void WriteAsync_ReadAsync_RoundTripsSameBytes()
        {
            string filePath = Path.Combine(scratchDir, "save.dat");
            byte[] data = { 1, 2, 3, 4, 5 };

            BFSaveFileIO.WriteAsync(filePath, data).GetAwaiter().GetResult();
            byte[] readBack = BFSaveFileIO.ReadAsync(filePath).GetAwaiter().GetResult();

            CollectionAssert.AreEqual(data, readBack);
        }

        [Test]
        public void ReadAsync_MissingFile_ReturnsNullAndLogsTrace()
        {
            SpyLoggerSink spy = InitializeLogging();
            string filePath = Path.Combine(scratchDir, "missing.dat");

            byte[] result = BFSaveFileIO.ReadAsync(filePath).GetAwaiter().GetResult();

            Assert.IsNull(result);
            Assert.IsTrue(spy.Entries.Exists(e => e.Message.Contains("No file found at")));
        }

        [Test]
        public void WriteAsync_ExistingFile_OverwritesWithNewContent()
        {
            string filePath = Path.Combine(scratchDir, "save.dat");
            BFSaveFileIO.WriteAsync(filePath, new byte[] { 1, 2, 3 }).GetAwaiter().GetResult();

            BFSaveFileIO.WriteAsync(filePath, new byte[] { 9, 8 }).GetAwaiter().GetResult();
            byte[] readBack = BFSaveFileIO.ReadAsync(filePath).GetAwaiter().GetResult();

            CollectionAssert.AreEqual(new byte[] { 9, 8 }, readBack);
        }
    }
}