using System.IO;
using System.Threading.Tasks;

namespace BFTools.Core.SaveSystem
{
    public static class BFSaveFileIO
    {
        public static async Task WriteAsync(string filePath, byte[] data)
        {
            string tempPath = filePath + ".tmp";

            using (FileStream fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await fileStream.WriteAsync(data, 0, data.Length);
            }

            if (File.Exists(filePath))
                File.Delete(filePath);

            File.Move(tempPath, filePath);
        }

        public static async Task<byte[]> ReadAsync(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            {
                byte[] buffer = new byte[fileStream.Length];
                await fileStream.ReadAsync(buffer, 0, buffer.Length);
                return buffer;
            }
        }
    }
}