using System.Buffers;

namespace ThreeL.Infra.Core.File
{
    public static class FileExtension
    {
        public async static Task<string> FileToBase64StringAsync(string fullName)
        {
            FileStream fsForRead = new FileStream(fullName, FileMode.Open);
            var array = ArrayPool<byte>.Shared.Rent((int)fsForRead.Length);
            try
            {
                fsForRead.Seek(0, SeekOrigin.Begin);
                int log = Convert.ToInt32(fsForRead.Length);
                await fsForRead.ReadAsync(array, 0, log);
                return Convert.ToBase64String(array);
            }
            catch
            {
                throw;
            }
            finally
            {
                fsForRead.Close();
                ArrayPool<byte>.Shared.Return(array);
            }
        }

        public async static Task<byte[]> Base64StringToFileAsync(string base64String, string fullPath)
        {
            MemoryStream stream = null;
            FileStream fs = null;
            try
            {
                string strbase64 = base64String.Trim().Substring(base64String.IndexOf(",") + 1);
                stream = new MemoryStream(Convert.FromBase64String(strbase64));
                fs = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Write);
                byte[] b = stream.ToArray();
                await fs.WriteAsync(b, 0, b.Length);

                return b;
            }
            catch
            {
                throw;
            }
            finally
            {
                stream?.Close();
                fs?.Close();
            }
        }
    }
}
