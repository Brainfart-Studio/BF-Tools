using System.IO;
using System.Security.Cryptography;
using System.Text;
using BFTools.Core.Logger;

namespace BFTools.Systems.SaveSystem
{
    public static class BFSaveKeyProvider
    {
        private const string LogTag = "Save";
        private const int KeySizeInBytes = 32;

        private static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

        private static byte[] cachedKey;
        private static byte[] cachedMacKey;

        public static byte[] GetKey()
        {
            if (cachedKey != null)
                return cachedKey;

            string keyPath = BFSavePath.KeyFilePath;

            if (File.Exists(keyPath))
            {
                cachedKey = File.ReadAllBytes(keyPath);
                BFLogger.Trace(LogTag, $"Loaded existing save key from '{keyPath}'");
            }
            else
            {
                cachedKey = GenerateAndPersistKey(keyPath);
                BFLogger.Info(LogTag, $"Generated new save key at '{keyPath}'");
            }

            return cachedKey;
        }

        public static byte[] GetMacKey()
        {
            if (cachedMacKey != null)
                return cachedMacKey;

            using (HMACSHA256 hmac = new HMACSHA256(GetKey()))
            {
                cachedMacKey = hmac.ComputeHash(Encoding.UTF8.GetBytes("BFSaveSystem.MacKey"));
            }

            return cachedMacKey;
        }

        public static void FillRandomBytes(byte[] buffer)
        {
            rng.GetBytes(buffer);
        }

        private static byte[] GenerateAndPersistKey(string keyPath)
        {
            byte[] key = new byte[KeySizeInBytes];
            rng.GetBytes(key);

            string tempPath = keyPath + ".tmp";
            File.WriteAllBytes(tempPath, key);

            if (File.Exists(keyPath))
                File.Delete(keyPath);

            File.Move(tempPath, keyPath);

            return key;
        }
    }
}