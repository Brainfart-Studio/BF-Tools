using System.Collections.Generic;

namespace BFTools.Core.SaveSystem
{
    public static class BFSaveManager
    {
        private static readonly List<ISaveable> saveables = new List<ISaveable>();

        public static void Register(ISaveable saveable)
        {
            if (!saveables.Contains(saveable))
                saveables.Add(saveable);
        }

        public static void Unregister(ISaveable saveable)
        {
            saveables.Remove(saveable);
        }
    }
}