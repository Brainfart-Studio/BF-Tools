using System.IO;
using System.Threading.Tasks;
using BFTools.Core.FileIO;
using BFTools.Core.Logger;

namespace BFTools.Systems.SaveSystem
{
    public static class BFSaveFileIO
    {
        private const string LogTag = "Save";

        public static async Task WriteAsync(string filePath, byte[] data)
        {
            BFLogger.Trace(LogTag, $"Writing {data.Length} byte(s) to '{filePath}'");

            string tempPath = filePath + ".tmp";

            using (FileStream fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await fileStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            }

            BFAtomicFile.Replace(tempPath, filePath);

            BFLogger.Trace(LogTag, $"Wrote '{filePath}'");
        }

        public static async Task<byte[]> ReadAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                BFLogger.Trace(LogTag, $"No file found at '{filePath}'");
                return null;
            }

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            {
                byte[] buffer = new byte[fileStream.Length];
                await fileStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                BFLogger.Trace(LogTag, $"Read {buffer.Length} byte(s) from '{filePath}'");
                return buffer;
            }
        }
    }
}