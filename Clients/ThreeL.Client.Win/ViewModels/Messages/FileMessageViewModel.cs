using CommunityToolkit.Mvvm.Input;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ThreeL.Client.Shared.Database;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Client.Shared.Entities;
using ThreeL.Client.Shared.Services;
using ThreeL.Client.Win.BackgroundService;
using ThreeL.Client.Win.Helpers;
using ThreeL.Infra.Core.Metadata;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.ViewModels.Messages
{
    public class FileMessageViewModel : MessageViewModel
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string FileSizeText => App.ServiceProvider.GetRequiredService<FileHelper>().SizeConvertText(FileSize);
        public string Location { get; set; }
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
            CopyCommandAsync = new AsyncRelayCommand(CopyAsync);
            OpenLocationCommandAsync = new AsyncRelayCommand(OpenLocationAsync);
        }

        public FileMessageViewModel()
        {
            CopyCommandAsync = new AsyncRelayCommand(CopyAsync);
            OpenLocationCommandAsync = new AsyncRelayCommand(OpenLocationAsync);
        }

        public override string GetShortDesc()
        {
            return "[文件]";
        }

        private async Task CopyAsync()
        {
            if (!string.IsNullOrEmpty(Location) && File.Exists(Location))
            {
                SetFileDrop(Location);
                return;
            }

            var dbConnection = App.ServiceProvider.GetRequiredService<ClientSqliteContext>();
            //数据库是否存在
            var record = await SqlMapper.QueryFirstOrDefaultAsync<ChatRecord>(dbConnection.dbConnection, "SELECT * FROM ChatRecord WHERE MessageId = @Id",
                                           new { Id = MessageId });

            if (record != null && !string.IsNullOrEmpty(record.ResourceLocalLocation) && File.Exists(record.ResourceLocalLocation))
            {
                Location = record.ResourceLocalLocation;
                SetFileDrop(Location);
                return;
            }

            var result = await DownloadAsync();
            if (!string.IsNullOrEmpty(result))
            {
                Location = result;
                SetFileDrop(Location);
            }
        }

        private async Task OpenLocationAsync() 
        {
            if (!string.IsNullOrEmpty(Location) && File.Exists(Location))
            {
                ExplorerFile(Location);
                return;
            }

            var dbConnection = App.ServiceProvider.GetRequiredService<ClientSqliteContext>();
            //数据库是否存在
            var record = await SqlMapper.QueryFirstOrDefaultAsync<ChatRecord>(dbConnection.dbConnection, "SELECT * FROM ChatRecord WHERE MessageId = @Id",
                                           new { Id = MessageId });

            if (record != null && !string.IsNullOrEmpty(record.ResourceLocalLocation) && File.Exists(record.ResourceLocalLocation))
            {
                Location = record.ResourceLocalLocation;
                ExplorerFile(Location);
                return;
            }

            var result = await DownloadAsync();
            if (!string.IsNullOrEmpty(result))
            {
                Location = result;
                ExplorerFile(Location);
            }
        }

        /// <summary>
        /// 直接下载文件
        /// </summary>
        /// <returns></returns>
        private async Task<string> DownloadAsync()
        {
            try
            {
                var bytes = await App.ServiceProvider.GetRequiredService<ContextAPIService>().DownloadFileAsync(MessageId);
                if (bytes == null)
                {
                    App.ServiceProvider.GetRequiredService<GrowlHelper>().Warning($"下载文件[{FileName}]失败，文件可能已过期");
                }

                var location = await App.ServiceProvider.GetRequiredService<FileHelper>()
                        .AutoSaveFileByBytesAsync(bytes, FileName, MessageType.File);
                try
                {
                    //更新或存数据库
                    var dbConnection = App.ServiceProvider.GetRequiredService<ClientSqliteContext>();
                    var record = await SqlMapper.QueryFirstOrDefaultAsync<ChatRecord>(dbConnection.dbConnection, "SELECT * FROM ChatRecord WHERE MessageId = @Id",
                                              new { Id = MessageId });

                    if (record == null)
                    {
                        await App.ServiceProvider.GetRequiredService<SaveChatRecordService>().WriteRecordAsync(new ChatRecord
                        {
                            From = From,
                            To = To,
                            MessageId = MessageId,
                            Message = FileName,
                            ResourceLocalLocation = location,
                            MessageRecordType = MessageRecordType.File,
                            SendTime = SendTime,
                            FileId = FileId
                        });
                    }
                    else
                    {
                        await SqlMapper.ExecuteAsync(dbConnection.dbConnection,
                                   "UPDATE ChatRecord SET ResourceLocalLocation = @Location WHERE MessageId = @Id",
                                   new { Id = record.MessageId, Location = location });
                    }

                    return location;
                }
                catch
                {
                    return location;
                }
            }
            catch
            {
                return null;
            }
        }

        public override void FromDto(ChatRecordResponseDto chatRecord)
        {
            base.FromDto(chatRecord);
            FileName = chatRecord.FileName;
            FileSize = chatRecord.Size;
            FileId = chatRecord.FileId == null ? 0 : chatRecord.FileId.Value;
            Source = GenerateIconByFileType();
        }

        private BitmapImage GenerateIconByFileType()
        {
            var source = new BitmapImage();
            try
            {
                string imgUrl = $"pack://application:,,,/ThreeL.Client.Win;component/Images/{ImageName}";
                source.BeginInit();
                source.UriSource = new Uri(imgUrl, UriKind.RelativeOrAbsolute);
                source.EndInit();

                return source;
            }
            finally
            {
                source.Freeze();
            }
        }
    }
}
