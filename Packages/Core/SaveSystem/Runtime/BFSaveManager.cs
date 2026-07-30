using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BFTools.Core.SaveSystem
{
    public static class BFSaveManager
    {
        private static readonly List<ISaveable> saveables = new List<ISaveable>();
        private static readonly List<BFSaveSlot> slots = new List<BFSaveSlot>();

        public static void Register(ISaveable saveable)
        {
            if (!saveables.Contains(saveable))
                saveables.Add(saveable);
        }

        public static void Unregister(ISaveable saveable)
        {
            saveables.Remove(saveable);
        }

        public static void RegisterSlot(BFSaveSlot slot)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].slotName == slot.slotName)
                {
                    slots[i] = slot;
                    return;
                }
            }

            slots.Add(slot);
        }

        public static bool TryGetSlot(string slotName, out BFSaveSlot slot)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].slotName == slotName)
                {
                    slot = slots[i];
                    return true;
                }
            }

            slot = default;
            return false;
        }

        public static string GetFileNameForSlot(string slotName)
        {
            return $"save_{slotName}.dat";
        }

        public static async Task SaveAsync(string slotName, string directoryPath)
        {
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
        }

        public static async Task<bool> LoadAsync(string slotName, string directoryPath)
        {
            string filePath = System.IO.Path.Combine(directoryPath, GetFileNameForSlot(slotName));
            string checksumPath = filePath + ".chk";

            byte[] encryptedBytes = await BFSaveFileIO.ReadAsync(filePath);
            byte[] checksumBytes = await BFSaveFileIO.ReadAsync(checksumPath);

            if (encryptedBytes == null || checksumBytes == null)
                return false;

            string expectedChecksum = System.Text.Encoding.UTF8.GetString(checksumBytes);
            if (!BFSaveChecksum.Validate(encryptedBytes, expectedChecksum))
                return false;

            string json = BFSaveEncryptor.Decrypt(encryptedBytes);
            BFSaveData saveData = BFSaveSerializer.Deserialize<BFSaveData>(json);

            object migrated = BFSaveVersionMigrator.Migrate(saveData, saveData.metadata.version);
            saveData = (BFSaveData)migrated;

            for (int i = 0; i < saveables.Count; i++)
            {
                ISaveable saveable = saveables[i];
                string key = saveable.GetType().Name;

                if (saveData.saveableStates.TryGetValue(key, out object state))
                    saveable.RestoreState(state);
            }

            RegisterSlot(new BFSaveSlot
            {
                slotName = slotName,
                metadata = saveData.metadata
            });

            return true;
        }
    }
}