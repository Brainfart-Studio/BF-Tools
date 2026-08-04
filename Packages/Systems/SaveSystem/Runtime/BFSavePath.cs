using UnityEngine;

namespace BFTools.Systems.SaveSystem
{
    public static class BFSavePath
    {
        public static string DefaultDirectory => Application.persistentDataPath;

        public static string KeyFilePath => System.IO.Path.Combine(DefaultDirectory, "save.key");
    }
}