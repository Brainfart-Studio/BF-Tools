using System.Security.Cryptography;
using System.Text;

namespace BFTools.Core.SaveSystem
{
    public static class BFSaveChecksum
    {
        public static string Generate(byte[] data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(data);
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < hash.Length; i++)
                    builder.Append(hash[i].ToString("x2"));

                return builder.ToString();
            }
        }

        public static bool Validate(byte[] data, string expectedChecksum)
        {
            string actualChecksum = Generate(data);
            return actualChecksum == expectedChecksum;
        }
    }
}