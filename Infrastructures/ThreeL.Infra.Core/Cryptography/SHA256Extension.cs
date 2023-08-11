using System.Security.Cryptography;

namespace ThreeL.Infra.Core.Cryptography
{
    public static class SHA256Extension
    {
        public static string ToSHA256(this byte[] data)
        {
            string code = string.Empty;
            using (SHA256 mySHA256 = SHA256.Create())
            {
                var sha256Bytes = mySHA256.ComputeHash(data);
                code = BitConverter.ToString(sha256Bytes).Replace("-", "").ToUpper();
            }

            return code;
        }

        public static string ToSHA256(this Stream stream)
        {
            string code = string.Empty;
            using (SHA256 mySHA256 = SHA256.Create())
            {
                var sha256Bytes = mySHA256.ComputeHash(stream);
                code = BitConverter.ToString(sha256Bytes).Replace("-", "").ToUpper();
            }

            return code;
        }
    }
}
