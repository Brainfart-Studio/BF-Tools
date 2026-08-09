using System.Security.Cryptography;
using System.Text;

namespace BFTools.Systems.SaveSystem
{
    internal static class BFSaveChecksum
    {
        public static string Generate(byte[] data)
        {
            using (HMACSHA256 hmac = new HMACSHA256(BFSaveKeyProvider.GetMacKey()))
            {
                byte[] hash = hmac.ComputeHash(data);
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