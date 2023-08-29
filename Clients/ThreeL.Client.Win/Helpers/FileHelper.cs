using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using ThreeL.Client.Shared.Services;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.Helpers
{
    public class FileHelper
    {
        private readonly ILogger<FileHelper> _logger;

        public FileHelper(ILogger<FileHelper> logger)
        {
            _logger = logger;
        }

        public async Task<string> AutoSaveFileByBytesAsync(byte[] bytes, string fileName, MessageType message)
        {
            try
            {
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);//TODO根据设置，获取用户选择的文件存放目录
                var location = string.Empty;
                if (message == MessageType.Image)
                {
                    location = Path.Combine(folder, "ThreeL/images", DateTime.Now.ToString("yyyy-MM"));
                }
                else if (message == MessageType.File)
                {
                    location = Path.Combine(folder, "ThreeL/files", DateTime.Now.ToString("yyyy-MM"));
                }

                if (!Directory.Exists(location))
                {
                    Directory.CreateDirectory(location);
                }

                var newfilename = GenerateFileName(location, fileName);
                var fileLocation = Path.Combine(location, newfilename);
                await File.WriteAllBytesAsync(fileLocation, bytes);

                return fileLocation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return default;
            }
        }

        private string GenerateFileName(string folderName, string fileName)
        {
            var justName = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var result = fileName;
            int index = 0;
            while (File.Exists(Path.Combine(folderName, result)))
            {
                result = $"{justName}({++index}){extension}";
            }

            return result;
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

        public void OpenFile(string fileName)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(fileName);
            Process process = new Process();
            process.StartInfo = processStartInfo;
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }

        public string SaveImageToFile(BitmapSource image, string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                var folder = Path.Combine(Path.GetTempPath(), "ThreeL");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                path = Path.Combine(folder, $"{Guid.NewGuid()}.png");
            }
            BitmapEncoder encoder = GetBitmapEncoder(path);
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var stream = new FileStream(path, FileMode.Create))
            {
                encoder.Save(stream);
            }

            return path;
        }

        private BitmapEncoder GetBitmapEncoder(string filePath)
        {
            var extName = Path.GetExtension(filePath).ToLower();
            if (extName.Equals(".png"))
            {
                return new PngBitmapEncoder();
            }
            else
            {
                return new JpegBitmapEncoder();
            }
        }

        public void RefreshAvatarAsync(long userId, long id)
        {
            var _ = Task.Run(async () =>
            {
                var data = await App.ServiceProvider.GetRequiredService<ContextAPIService>().DownloadUserAvatarAsync(userId, id);
                if (data != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        return App.ServiceProvider.GetRequiredService<FileHelper>().ByteArrayToBitmapImage(data);
                    });
                }
            });
        }
    }
}
