using NUnit.Framework;
using BFTools.Systems.SaveSystem;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Systems.SaveSystem.Tests
{
    public class BFSaveEncryptorTests
    {
        [Test]
        public void Encrypt_Decrypt_RoundTripsOriginalPlainText()
        {
            string plainText = "{\"level\":3,\"gold\":150}";

            byte[] cipherBytes = BFSaveEncryptor.Encrypt(plainText);
            string decrypted = BFSaveEncryptor.Decrypt(cipherBytes);

            Assert.AreEqual(plainText, decrypted);
        }

        [Test]
        public void Encrypt_EmptyString_RoundTrips()
        {
            byte[] cipherBytes = BFSaveEncryptor.Encrypt(string.Empty);
            string decrypted = BFSaveEncryptor.Decrypt(cipherBytes);

            Assert.AreEqual(string.Empty, decrypted);
        }

        [Test]
        public void Encrypt_SameInputTwice_ProducesDifferentCiphertextButSameDecryptedResult()
        {
            string plainText = "save-payload";

            byte[] first = BFSaveEncryptor.Encrypt(plainText);
            byte[] second = BFSaveEncryptor.Encrypt(plainText);

            CollectionAssert.AreNotEqual(first, second);
            Assert.AreEqual(plainText, BFSaveEncryptor.Decrypt(first));
            Assert.AreEqual(plainText, BFSaveEncryptor.Decrypt(second));
        }

        [Test]
        public void Encrypt_OutputIsLongerThanIvLength()
        {
            byte[] cipherBytes = BFSaveEncryptor.Encrypt("save-payload");

            Assert.Greater(cipherBytes.Length, 16);
        }
    }
}