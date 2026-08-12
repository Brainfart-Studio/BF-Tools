using System.IO;
using System.Reflection;
using NUnit.Framework;
using BFTools.Core.FileIO;

namespace BFTools.Systems.SaveSystem.Tests
{
    [SetUpFixture]
    public class BFSaveSystemTestSetup
    {
        private static string scratchDir;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            scratchDir = Path.Combine(Path.GetTempPath(), "BFSaveSystemTests_Keys");
            if (Directory.Exists(scratchDir))
                Directory.Delete(scratchDir, true);
            Directory.CreateDirectory(scratchDir);

            typeof(BFPersistentDataPath)
                .GetField("directoryOverride", BindingFlags.NonPublic | BindingFlags.Static)
                .SetValue(null, scratchDir);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            typeof(BFPersistentDataPath)
                .GetField("directoryOverride", BindingFlags.NonPublic | BindingFlags.Static)
                .SetValue(null, null);

            if (Directory.Exists(scratchDir))
                Directory.Delete(scratchDir, true);
        }
    }
}