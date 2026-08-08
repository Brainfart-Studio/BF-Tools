using System.IO;

namespace BFTools.Core.FileIO
{
    public static class BFAtomicFile
    {
        public static void Replace(string sourcePath, string destinationPath)
        {
            if (File.Exists(destinationPath))
                File.Replace(sourcePath, destinationPath, null);
            else
                File.Move(sourcePath, destinationPath);
        }
    }
}