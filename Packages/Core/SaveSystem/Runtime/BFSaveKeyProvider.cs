using System.IO;
using System.Security.Cryptography;
using BFTools.Core.Logger;

namespace BFTools.Core.SaveSystem
{
    public static class BFSaveKeyProvider
    {
        private const string LogTag = "Save";
        private const int KeySizeInBytes = 32;

        private static byte[] cachedKey;

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

        private static byte[] GenerateAndPersistKey(string keyPath)
        {
            byte[] key = new byte[KeySizeInBytes];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }

            string tempPath = keyPath + ".tmp";
            File.WriteAllBytes(tempPath, key);

            if (File.Exists(keyPath))
                File.Delete(keyPath);

            File.Move(tempPath, keyPath);

            return key;
        }
    }
}