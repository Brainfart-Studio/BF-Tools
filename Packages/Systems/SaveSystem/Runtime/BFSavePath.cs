using UnityEngine;

namespace BFTools.Systems.SaveSystem
{
    public static class BFSavePath
    {
        private static string directoryOverride;

        public static string DefaultDirectory => directoryOverride ?? Application.persistentDataPath;

        public static string KeyFilePath => System.IO.Path.Combine(DefaultDirectory, "save.key");
    }
}