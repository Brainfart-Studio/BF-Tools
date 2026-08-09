using System;
using System.IO;
using System.Security.Cryptography;

namespace BFTools.Systems.SaveSystem
{
    internal static class BFSaveEncryptor
    {
        public static byte[] Encrypt(string plainText)
        {
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
            using (Aes aes = Aes.Create())
            {
                aes.Key = BFSaveKeyProvider.GetKey();

                int ivLength = aes.BlockSize / 8;
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