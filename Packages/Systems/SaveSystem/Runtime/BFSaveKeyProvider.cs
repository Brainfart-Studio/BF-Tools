using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BFTools.Core.Logger;

namespace BFTools.Systems.SaveSystem
{
    public static class BFSaveKeyProvider
    {
        private const string LogTag = "Save";
        private const int KeySizeInBytes = 32;
        private const int RandomGenerationTimeoutMs = 2000;

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
            byte[] secureBytes = null;
            Task task = Task.Run(() =>
            {
                byte[] temp = new byte[buffer.Length];
                rng.GetBytes(temp);
                secureBytes = temp;
            });

            if (task.Wait(RandomGenerationTimeoutMs) && secureBytes != null)
            {
                Array.Copy(secureBytes, buffer, buffer.Length);
                return;
            }

            BFLogger.Warning(LogTag, "Secure RNG did not respond in time (likely low system entropy); falling back to a non-blocking pseudo-random seed.");
            FillFallbackRandomBytes(buffer);
        }

        private static void FillFallbackRandomBytes(byte[] buffer)
        {
            byte[] seedMaterial = Guid.NewGuid().ToByteArray();
            int seed = BitConverter.ToInt32(seedMaterial, 0) ^ Environment.TickCount ^ (int)DateTime.UtcNow.Ticks;
            new Random(seed).NextBytes(buffer);
        }

        private static byte[] GenerateAndPersistKey(string keyPath)
        {
            byte[] key = new byte[KeySizeInBytes];
            FillRandomBytes(key);

            string tempPath = keyPath + ".tmp";
            File.WriteAllBytes(tempPath, key);

            if (File.Exists(keyPath))
                File.Delete(keyPath);

            File.Move(tempPath, keyPath);

            return key;
        }
    }
}