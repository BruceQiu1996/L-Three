using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Windows.Media.Imaging;
using ThreeL.Client.Shared.Entities;
using ThreeL.Client.Win.Helpers;

namespace ThreeL.Client.Win.ViewModels.Messages
{
    public class FileMessageViewModel : MessageViewModel
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string FileSizeText => App.ServiceProvider.GetRequiredService<FileHelper>().SizeConvertText(FileSize);
        public long FileId { get; set; }
        public string ImageName => Path.GetExtension(FileName)?.ToLower() switch
        {
            ".zip" => "zip.png",
            ".7z" => "zip.png",
            ".tar" => "zip.png",
            ".rar" => "zip.png",
            ".xls" => "excel.png",
            ".xlsx" => "excel.png",
            ".html" => "html.png",
            ".txt" => "txt.png",
            ".doc" => "word.png",
            ".docx" => "word.png",
            ".dll" => "dll.png",
            ".pdf" => "pdf.png",
            _ => "unknown.png"
        };

        public BitmapImage Source { get; set; }

        public FileMessageViewModel(string fileName)
        {
            FileName = fileName;
            Source = GenerateIconByFileType();
        }

        public override string GetShortDesc()
        {
            return "[文件]";
        }

        public override void FromEntity(ChatRecord chatRecord)
        {
            base.FromEntity(chatRecord);
            FileSize = chatRecord.ResourceSize == null ? 0 : chatRecord.ResourceSize.Value;
            FileId = chatRecord.FileId == null ? 0 : chatRecord.FileId.Value;
            Source = GenerateIconByFileType();
        }

        private BitmapImage GenerateIconByFileType() 
        {
            var Source = new BitmapImage();
            string imgUrl = $"pack://application:,,,/ThreeL.Client.Win;component/Images/{ImageName}";
            Source.BeginInit();
            Source.UriSource = new Uri(imgUrl, UriKind.RelativeOrAbsolute);
            Source.EndInit();

            return Source;
        }
    }
}
