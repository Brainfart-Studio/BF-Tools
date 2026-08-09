using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Systems.SaveSystem;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Systems.SaveSystem.Tests
{
    public class BFSavePathTests
    {
        private static readonly FieldInfo DirectoryOverrideField =
            typeof(BFSavePath).GetField("directoryOverride", BindingFlags.NonPublic | BindingFlags.Static);

        private string previousOverride;

        [SetUp]
        public void SetUp()
        {
            previousOverride = (string)DirectoryOverrideField.GetValue(null);
        }

        [TearDown]
        public void TearDown()
        {
            DirectoryOverrideField.SetValue(null, previousOverride);
        }

        [Test]
        public void DefaultDirectory_NoOverride_FallsBackToPersistentDataPath()
        {
            DirectoryOverrideField.SetValue(null, null);

            Assert.AreEqual(Application.persistentDataPath, BFSavePath.DefaultDirectory);
        }

        [Test]
        public void DefaultDirectory_OverrideSet_ReturnsOverride()
        {
            string overridePath = Path.Combine(Path.GetTempPath(), "BFSavePathTests_Override");
            DirectoryOverrideField.SetValue(null, overridePath);

            Assert.AreEqual(overridePath, BFSavePath.DefaultDirectory);
        }

        [Test]
        public void KeyFilePath_CombinesDefaultDirectoryWithFixedFileName()
        {
            string overridePath = Path.Combine(Path.GetTempPath(), "BFSavePathTests_KeyDir");
            DirectoryOverrideField.SetValue(null, overridePath);

            string expected = Path.Combine(overridePath, "save.key");

            Assert.AreEqual(expected, BFSavePath.KeyFilePath);
        }
    }
}