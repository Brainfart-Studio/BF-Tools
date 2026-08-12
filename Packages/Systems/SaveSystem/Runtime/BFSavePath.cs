using BFTools.Core.FileIO;

namespace BFTools.Systems.SaveSystem
{
    public static class BFSavePath
    {
        public static string DefaultDirectory => BFPersistentDataPath.Directory;

        public static string KeyFilePath => System.IO.Path.Combine(DefaultDirectory, "save.key");
    }
}