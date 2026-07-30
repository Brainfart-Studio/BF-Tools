using System.Collections.Generic;

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
    }
}