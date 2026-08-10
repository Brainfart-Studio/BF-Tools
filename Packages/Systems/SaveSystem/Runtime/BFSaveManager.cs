using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFTools.Core.FileIO;
using BFTools.Core.Logger;
using BFTools.Core.Serialization;

namespace BFTools.Systems.SaveSystem
{
    public static class BFSaveManager
    {
        private const string LogTag = "Save";

        private static readonly BFAllowlistJsonSerializer serializer = new BFAllowlistJsonSerializer(LogTag);
        private static readonly BFStateRegistry<ISaveable> saveables = new BFStateRegistry<ISaveable>(LogTag, serializer);
        private static readonly object slotsLock = new object();
        private static readonly List<BFSaveSlot> slots = new List<BFSaveSlot>();

        public static void Register(ISaveable saveable)
        {
            saveables.Register(saveable);
        }

        public static void Unregister(ISaveable saveable)
        {
            saveables.Unregister(saveable);
        }

        public static void RegisterSlot(BFSaveSlot slot)
        {
            lock (slotsLock)
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i].slotName == slot.slotName)
                    {
                        slots[i] = slot;
                        BFLogger.Trace(LogTag, $"Updated slot '{slot.slotName}'");
                        return;
                    }
                }

                slots.Add(slot);
                BFLogger.Trace(LogTag, $"Registered new slot '{slot.slotName}'");
            }
        }

        public static bool TryGetSlot(string slotName, out BFSaveSlot slot)
        {
            lock (slotsLock)
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i].slotName == slotName)
                    {
                        slot = slots[i];
                        BFLogger.Trace(LogTag, $"Found slot '{slotName}'");
                        return true;
                    }
                }

                slot = default;
                BFLogger.Trace(LogTag, $"No slot found for '{slotName}'");
                return false;
            }
        }

        public static string GetFileNameForSlot(string slotName)
        {
            if (string.IsNullOrWhiteSpace(slotName))
                throw new ArgumentException("Slot name must not be null or empty.", nameof(slotName));

            if (slotName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0
                || slotName.IndexOf('/') >= 0
                || slotName.IndexOf('\\') >= 0
                || slotName.Contains(".."))
            {
                throw new ArgumentException($"Slot name '{slotName}' contains characters that are not allowed in a file name.", nameof(slotName));
            }

            return $"save_{slotName}.dat";
        }

        public static Task<bool> SaveAsync(string slotName)
        {
            return SaveAsync(slotName, BFSavePath.DefaultDirectory);
        }

        public static Task<bool> LoadAsync(string slotName)
        {
            return LoadAsync(slotName, BFSavePath.DefaultDirectory);
        }

        public static async Task<bool> SaveAsync(string slotName, string directoryPath)
        {
            BFLogger.Trace(LogTag, $"SaveAsync started for slot '{slotName}'");

            BFSaveData saveData = new BFSaveData
            {
                metadata = new BFSaveMetadata
                {
                    version = BFSaveVersionMigrator.CurrentVersion,
                    timestamp = DateTime.UtcNow,
                    playtimeSeconds = 0f
                }
            };

            string filePath = System.IO.Path.Combine(directoryPath, GetFileNameForSlot(slotName));
            string checksumPath = filePath + ".chk";

            try
            {
                saveData.saveableStates = saveables.CaptureAll($" for slot '{slotName}'");

                string json = serializer.Serialize(saveData);
                byte[] encryptedBytes = BFSaveEncryptor.Encrypt(json);
                string checksum = BFSaveChecksum.Generate(encryptedBytes);

                byte[] checksumBytes = System.Text.Encoding.UTF8.GetBytes(checksum);

                BFLogger.Trace(LogTag, $"Writing {encryptedBytes.Length} byte(s) to '{filePath}'");
                await BFFileIO.WriteAsync(filePath, encryptedBytes).ConfigureAwait(false);
                BFLogger.Trace(LogTag, $"Wrote '{filePath}'");

                BFLogger.Trace(LogTag, $"Writing {checksumBytes.Length} byte(s) to '{checksumPath}'");
                await BFFileIO.WriteAsync(checksumPath, checksumBytes).ConfigureAwait(false);
                BFLogger.Trace(LogTag, $"Wrote '{checksumPath}'");

                BFLogger.Trace(LogTag, $"SaveAsync completed for slot '{slotName}' ({encryptedBytes.Length} byte(s) written to '{filePath}')");
            }
            catch (Exception exception)
            {
                BFLogger.Warning(LogTag, $"Failed to save slot '{slotName}': {exception.Message}. The slot's on-disk files may now be missing or incomplete.");
                return false;
            }

            RegisterSlot(new BFSaveSlot
            {
                slotName = slotName,
                metadata = saveData.metadata
            });

            return true;
        }

        public static async Task<bool> LoadAsync(string slotName, string directoryPath)
        {
            BFLogger.Trace(LogTag, $"LoadAsync started for slot '{slotName}'");

            string filePath = System.IO.Path.Combine(directoryPath, GetFileNameForSlot(slotName));
            string checksumPath = filePath + ".chk";

            byte[] encryptedBytes = await BFFileIO.ReadAsync(filePath).ConfigureAwait(false);
            if (encryptedBytes == null)
                BFLogger.Trace(LogTag, $"No file found at '{filePath}'");
            else
                BFLogger.Trace(LogTag, $"Read {encryptedBytes.Length} byte(s) from '{filePath}'");

            byte[] checksumBytes = await BFFileIO.ReadAsync(checksumPath).ConfigureAwait(false);
            if (checksumBytes == null)
                BFLogger.Trace(LogTag, $"No file found at '{checksumPath}'");
            else
                BFLogger.Trace(LogTag, $"Read {checksumBytes.Length} byte(s) from '{checksumPath}'");

            if (encryptedBytes == null && checksumBytes == null)
            {
                BFLogger.Trace(LogTag, $"No save file found for slot '{slotName}', treating as first launch");
                return false;
            }

            if (encryptedBytes == null || checksumBytes == null)
            {
                string missing = encryptedBytes == null ? "save data" : "checksum";
                BFLogger.Warning(LogTag, $"Slot '{slotName}' is missing its {missing} file while the other is present. Save is incomplete or was interrupted mid-write.");
                return false;
            }

            string expectedChecksum = System.Text.Encoding.UTF8.GetString(checksumBytes);
            if (!BFSaveChecksum.Validate(encryptedBytes, expectedChecksum))
            {
                BFLogger.Warning(LogTag, $"Checksum mismatch for slot '{slotName}'. Save data may be corrupted or tampered with.");
                return false;
            }

            string json;
            BFSaveData saveData;

            try
            {
                json = BFSaveEncryptor.Decrypt(encryptedBytes);
                saveData = serializer.Deserialize<BFSaveData>(json);
            }
            catch (Exception exception)
            {
                BFLogger.Warning(LogTag, $"Failed to decrypt or parse save data for slot '{slotName}': {exception.Message}. Save may be corrupted, encrypted with a different key, or reference a type outside the save allowlist.");
                return false;
            }

            if (saveData == null)
            {
                BFLogger.Warning(LogTag, $"Decrypted save data for slot '{slotName}' was empty or invalid.");
                return false;
            }

            object migrated = BFSaveVersionMigrator.Migrate(saveData, saveData.metadata.version);
            saveData = (BFSaveData)migrated;

            saveables.RestoreAll(saveData.saveableStates, $" in slot '{slotName}'");

            RegisterSlot(new BFSaveSlot
            {
                slotName = slotName,
                metadata = saveData.metadata
            });

            BFLogger.Trace(LogTag, $"LoadAsync completed for slot '{slotName}'");

            return true;
        }
    }
}