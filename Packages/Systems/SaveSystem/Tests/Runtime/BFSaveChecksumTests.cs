using System.Text;
using NUnit.Framework;
using BFTools.Systems.SaveSystem;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Systems.SaveSystem.Tests
{
    public class BFSaveChecksumTests
    {
        [Test]
        public void Generate_SameInput_ProducesSameChecksum()
        {
            byte[] data = Encoding.UTF8.GetBytes("save-payload");

            string first = BFSaveChecksum.Generate(data);
            string second = BFSaveChecksum.Generate(data);

            Assert.AreEqual(first, second);
        }

        [Test]
        public void Generate_DifferentInput_ProducesDifferentChecksum()
        {
            string checksumA = BFSaveChecksum.Generate(Encoding.UTF8.GetBytes("save-payload-a"));
            string checksumB = BFSaveChecksum.Generate(Encoding.UTF8.GetBytes("save-payload-b"));

            Assert.AreNotEqual(checksumA, checksumB);
        }

        [Test]
        public void Generate_ReturnsLowercaseHexStringOfHmacSha256Length()
        {
            string checksum = BFSaveChecksum.Generate(Encoding.UTF8.GetBytes("save-payload"));

            Assert.AreEqual(64, checksum.Length);
            Assert.AreEqual(checksum, checksum.ToLowerInvariant());
        }

        [Test]
        public void Validate_MatchingChecksum_ReturnsTrue()
        {
            byte[] data = Encoding.UTF8.GetBytes("save-payload");
            string checksum = BFSaveChecksum.Generate(data);

            Assert.IsTrue(BFSaveChecksum.Validate(data, checksum));
        }

        [Test]
        public void Validate_TamperedData_ReturnsFalse()
        {
            string checksum = BFSaveChecksum.Generate(Encoding.UTF8.GetBytes("save-payload"));
            byte[] tamperedData = Encoding.UTF8.GetBytes("save-payload-tampered");

            Assert.IsFalse(BFSaveChecksum.Validate(tamperedData, checksum));
        }
    }
}