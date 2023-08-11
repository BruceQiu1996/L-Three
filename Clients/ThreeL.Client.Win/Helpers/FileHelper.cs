using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ThreeL.Client.Win.Helpers
{
    public class FileHelper
    {
        private readonly ILogger<FileHelper> _logger;
        public FileHelper(ILogger<FileHelper> logger)
        {
            _logger = logger;
        }

        public async Task<string> AutoSaveImageAsync(byte[] data, string fileName)
        {
            try
            {
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var location = Path.Combine(folder, "ThreeL/images", DateTime.Now.ToString("yyyy-MM"));
                if (!Directory.Exists(location))
                {
                    Directory.CreateDirectory(location);
                }

                var fileLocation = Path.Combine(location, $"{Guid.NewGuid()}{Path.GetExtension(fileName)}");
                await File.WriteAllBytesAsync(fileLocation, data);

                return fileLocation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return null;
            }
        }

        public BitmapImage ByteArrayToBitmapImage(byte[] byteArray)
        {
            BitmapImage bmp = null;
            try
            {
                bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = new MemoryStream(byteArray);
                bmp.EndInit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                bmp = null;
            }

            return bmp;
        }

        public string SizeConvertText(long size)
        {
            if (size == 0)
                return "0KB";

            if (size < 1024)
            {
                return $"{size}B";
            }
            else if (size >= 1024 && size < 1024 * 1024)
            {
                return $"{size / 1024}KB";
            }
            else
            {
                return $"{size / (1024 * 1024)}MB";
            }
        }
    }
}
