using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFTools.Core.Logger;

namespace BFTools.Core.SaveSystem
{
    public static class BFSaveManager
    {
        private const string LogTag = "Save";

        private static readonly List<ISaveable> saveables = new List<ISaveable>();
        private static readonly List<BFSaveSlot> slots = new List<BFSaveSlot>();
        private static readonly HashSet<Type> registeredStateTypes = new HashSet<Type>();

        public static void Register(ISaveable saveable)
        {
            if (!saveables.Contains(saveable))
            {
                saveables.Add(saveable);

                if (registeredStateTypes.Add(saveable.StateType))
                    BFSaveSerializer.AllowType(saveable.StateType);

                BFLogger.Trace(LogTag, $"Registered {saveable.GetType().Name}");
            }
        }

        public static void Unregister(ISaveable saveable)
        {
            if (saveables.Remove(saveable))
                BFLogger.Trace(LogTag, $"Unregistered {saveable.GetType().Name}");
        }

        public static void RegisterSlot(BFSaveSlot slot)
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

        public static bool TryGetSlot(string slotName, out BFSaveSlot slot)
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

        public static string GetFileNameForSlot(string slotName)
        {
            return $"save_{slotName}.dat";
        }

        public static Task SaveAsync(string slotName)
        {
            return SaveAsync(slotName, BFSavePath.DefaultDirectory);
        }

        public static Task<bool> LoadAsync(string slotName)
        {
            return LoadAsync(slotName, BFSavePath.DefaultDirectory);
        }

        public static async Task SaveAsync(string slotName, string directoryPath)
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

            for (int i = 0; i < saveables.Count; i++)
            {
                ISaveable saveable = saveables[i];
                saveData.saveableStates[saveable.GetType().Name] = saveable.CaptureState();
            }

            BFLogger.Debug(LogTag, $"Captured state from {saveables.Count} saveable(s) for slot '{slotName}'");

            string json = BFSaveSerializer.Serialize(saveData);
            byte[] encryptedBytes = BFSaveEncryptor.Encrypt(json);
            string checksum = BFSaveChecksum.Generate(encryptedBytes);

            string filePath = System.IO.Path.Combine(directoryPath, GetFileNameForSlot(slotName));
            string checksumPath = filePath + ".chk";

            await BFSaveFileIO.WriteAsync(filePath, encryptedBytes);
            await BFSaveFileIO.WriteAsync(checksumPath, System.Text.Encoding.UTF8.GetBytes(checksum));

            RegisterSlot(new BFSaveSlot
            {
                slotName = slotName,
                metadata = saveData.metadata
            });

            BFLogger.Trace(LogTag, $"SaveAsync completed for slot '{slotName}' ({encryptedBytes.Length} byte(s) written to '{filePath}')");
        }

        public static async Task<bool> LoadAsync(string slotName, string directoryPath)
        {
            BFLogger.Trace(LogTag, $"LoadAsync started for slot '{slotName}'");

            string filePath = System.IO.Path.Combine(directoryPath, GetFileNameForSlot(slotName));
            string checksumPath = filePath + ".chk";

            byte[] encryptedBytes = await BFSaveFileIO.ReadAsync(filePath);
            byte[] checksumBytes = await BFSaveFileIO.ReadAsync(checksumPath);

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
                saveData = BFSaveSerializer.Deserialize<BFSaveData>(json);
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

            if (saveData.metadata.version != BFSaveVersionMigrator.CurrentVersion)
                BFLogger.Info(LogTag, $"Migrating slot '{slotName}' from version {saveData.metadata.version} to {BFSaveVersionMigrator.CurrentVersion}");

            object migrated = BFSaveVersionMigrator.Migrate(saveData, saveData.metadata.version);
            saveData = (BFSaveData)migrated;

            int restoredCount = 0;
            for (int i = 0; i < saveables.Count; i++)
            {
                ISaveable saveable = saveables[i];
                string key = saveable.GetType().Name;

                if (saveData.saveableStates.TryGetValue(key, out object state))
                {
                    saveable.RestoreState(state);
                    restoredCount++;
                }
                else
                {
                    BFLogger.Trace(LogTag, $"No saved state found for '{key}' in slot '{slotName}', leaving as-is");
                }
            }

            BFLogger.Debug(LogTag, $"Restored state for {restoredCount} of {saveables.Count} saveable(s) from slot '{slotName}'");

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