using System;
using System.IO;
using System.Security.Cryptography;

namespace BFTools.Systems.SaveSystem
{
    internal static class BFSaveEncryptor
    {
        public static byte[] Encrypt(string plainText)
        {
            if (plainText == null)
                throw new ArgumentNullException(nameof(plainText));

            using (Aes aes = Aes.Create())
            {
                aes.Key = BFSaveKeyProvider.GetKey();

                byte[] iv = new byte[aes.BlockSize / 8];
                BFSaveKeyProvider.FillRandomBytes(iv);
                aes.IV = iv;

                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    memoryStream.Write(iv, 0, iv.Length);

                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter writer = new StreamWriter(cryptoStream))
                    {
                        writer.Write(plainText);
                    }

                    return memoryStream.ToArray();
                }
            }
        }

        public static string Decrypt(byte[] cipherBytes)
        {
            if (cipherBytes == null)
                throw new ArgumentNullException(nameof(cipherBytes));

            using (Aes aes = Aes.Create())
            {
                int ivLength = aes.BlockSize / 8;

                if (cipherBytes.Length < ivLength)
                    throw new ArgumentException($"Cipher data must be at least {ivLength} byte(s) long to contain an IV.", nameof(cipherBytes));

                aes.Key = BFSaveKeyProvider.GetKey();

                byte[] iv = new byte[ivLength];
                Array.Copy(cipherBytes, iv, ivLength);
                aes.IV = iv;

                using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (MemoryStream memoryStream = new MemoryStream(cipherBytes, ivLength, cipherBytes.Length - ivLength))
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                using (StreamReader reader = new StreamReader(cryptoStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}