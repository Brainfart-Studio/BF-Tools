using System;
using System.IO;

namespace BFTools.Core.FileIO
{
    public static class BFAtomicFile
    {
        public static void Replace(string sourcePath, string destinationPath)
        {
            if (string.IsNullOrEmpty(sourcePath))
                throw new ArgumentException("Source path must not be null or empty.", nameof(sourcePath));

            if (string.IsNullOrEmpty(destinationPath))
                throw new ArgumentException("Destination path must not be null or empty.", nameof(destinationPath));

            if (!File.Exists(sourcePath))
                throw new FileNotFoundException($"Cannot replace '{destinationPath}': source file '{sourcePath}' does not exist.", sourcePath);

            if (File.Exists(destinationPath))
                File.Replace(sourcePath, destinationPath, null);
            else
                File.Move(sourcePath, destinationPath);
        }
    }
}