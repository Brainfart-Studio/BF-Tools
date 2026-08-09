using System;
using System.IO;
using NUnit.Framework;
using BFTools.Core.FileIO;

namespace BFTools.Core.FileIO.Tests
{
    public class BFFileIOTests
    {
        private string scratchDir;

        [SetUp]
        public void SetUp()
        {
            scratchDir = Path.Combine(Path.GetTempPath(), "BFFileIOTests");
            if (Directory.Exists(scratchDir))
                Directory.Delete(scratchDir, true);
            Directory.CreateDirectory(scratchDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(scratchDir))
                Directory.Delete(scratchDir, true);
        }

        [Test]
        public void WriteAsync_ReadAsync_RoundTripsSameBytes()
        {
            string filePath = Path.Combine(scratchDir, "data.bin");
            byte[] data = { 1, 2, 3, 4, 5 };

            BFFileIO.WriteAsync(filePath, data).GetAwaiter().GetResult();
            byte[] readBack = BFFileIO.ReadAsync(filePath).GetAwaiter().GetResult();

            CollectionAssert.AreEqual(data, readBack);
        }

        [Test]
        public void ReadAsync_MissingFile_ReturnsNull()
        {
            string filePath = Path.Combine(scratchDir, "missing.bin");

            byte[] result = BFFileIO.ReadAsync(filePath).GetAwaiter().GetResult();

            Assert.IsNull(result);
        }

        [Test]
        public void WriteAsync_ExistingFile_OverwritesWithNewContent()
        {
            string filePath = Path.Combine(scratchDir, "data.bin");
            BFFileIO.WriteAsync(filePath, new byte[] { 1, 2, 3 }).GetAwaiter().GetResult();

            BFFileIO.WriteAsync(filePath, new byte[] { 9, 8 }).GetAwaiter().GetResult();
            byte[] readBack = BFFileIO.ReadAsync(filePath).GetAwaiter().GetResult();

            CollectionAssert.AreEqual(new byte[] { 9, 8 }, readBack);
        }

        [Test]
        public void WriteAsync_NullPath_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => BFFileIO.WriteAsync(null, new byte[] { 1 }).GetAwaiter().GetResult());
        }

        [Test]
        public void WriteAsync_NullData_ThrowsArgumentNullException()
        {
            string filePath = Path.Combine(scratchDir, "data.bin");

            Assert.Throws<ArgumentNullException>(() => BFFileIO.WriteAsync(filePath, null).GetAwaiter().GetResult());
        }

        [Test]
        public void ReadAsync_NullPath_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => BFFileIO.ReadAsync(null).GetAwaiter().GetResult());
        }
    }
}