using System;
using System.IO;
using System.Threading.Tasks;

namespace BFTools.Core.FileIO
{
    public static class BFFileIO
    {
        public static async Task WriteAsync(string filePath, byte[] data)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path must not be null or empty.", nameof(filePath));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            string tempPath = filePath + ".tmp";

            using (FileStream fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await fileStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            }

            BFAtomicFile.Replace(tempPath, filePath);
        }

        public static async Task<byte[]> ReadAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path must not be null or empty.", nameof(filePath));

            if (!File.Exists(filePath))
                return null;

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            {
                byte[] buffer = new byte[fileStream.Length];
                await fileStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                return buffer;
            }
        }
    }
}