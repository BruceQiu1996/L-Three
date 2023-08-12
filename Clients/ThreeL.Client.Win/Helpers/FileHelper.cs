using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ThreeL.Infra.Core.File;

namespace ThreeL.Client.Win.Helpers
{
    public class FileHelper
    {
        private readonly ILogger<FileHelper> _logger;
        public FileHelper(ILogger<FileHelper> logger)
        {
            _logger = logger;
        }

        public async Task<(byte[] raw, string location)> AutoSaveImageAsync(string base64, string fileName)
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
                var bytes = await FileExtension.Base64StringToFileAsync(base64, fileLocation);

                return (bytes, fileLocation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return default;
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
            finally 
            {
                bmp.Freeze();
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
