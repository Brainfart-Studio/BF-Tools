using UnityEngine;

namespace BFTools.Core.FileIO
{
    public static class BFPersistentDataPath
    {
        private static string directoryOverride;

        public static string Directory => directoryOverride ?? Application.persistentDataPath;
    }
}