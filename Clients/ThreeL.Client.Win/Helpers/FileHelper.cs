using System;
using System.IO;
using System.Threading.Tasks;

namespace ThreeL.Client.Win.Helpers
{
    public class FileHelper
    {
        public async Task AutoSaveImageAsync(byte[] data, string fileName)
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var location = Path.Combine(folder, "ThreeL/images", DateTime.Now.ToString("yyyy-MM"));
            if (!Directory.Exists(location))
            {
                Directory.CreateDirectory(location);
            }

            await File.WriteAllBytesAsync(Path.Combine(location, $"{Guid.NewGuid()}{Path.GetExtension(fileName)}"), data);
        }
    }
}
