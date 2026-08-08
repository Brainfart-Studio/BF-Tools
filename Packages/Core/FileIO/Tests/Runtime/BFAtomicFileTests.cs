using System.IO;
using NUnit.Framework;
using BFTools.Core.FileIO;

namespace BFTools.Core.FileIO.Tests
{
    public class BFAtomicFileTests
    {
        private string scratchDir;
        private string sourcePath;
        private string destinationPath;

        [SetUp]
        public void SetUp()
        {
            scratchDir = Path.Combine(Path.GetTempPath(), "BFAtomicFileTests");
            if (Directory.Exists(scratchDir))
                Directory.Delete(scratchDir, true);
            Directory.CreateDirectory(scratchDir);

            sourcePath = Path.Combine(scratchDir, "source.txt");
            destinationPath = Path.Combine(scratchDir, "destination.txt");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(scratchDir))
                Directory.Delete(scratchDir, true);
        }

        [Test]
        public void Replace_DestinationDoesNotExist_MovesSourceToDestination()
        {
            File.WriteAllText(sourcePath, "source content");

            BFAtomicFile.Replace(sourcePath, destinationPath);

            Assert.IsFalse(File.Exists(sourcePath));
            Assert.IsTrue(File.Exists(destinationPath));
            Assert.AreEqual("source content", File.ReadAllText(destinationPath));
        }

        [Test]
        public void Replace_DestinationExists_OverwritesWithSourceContent()
        {
            File.WriteAllText(sourcePath, "new content");
            File.WriteAllText(destinationPath, "old content");

            BFAtomicFile.Replace(sourcePath, destinationPath);

            Assert.IsFalse(File.Exists(sourcePath));
            Assert.AreEqual("new content", File.ReadAllText(destinationPath));
        }
    }
}