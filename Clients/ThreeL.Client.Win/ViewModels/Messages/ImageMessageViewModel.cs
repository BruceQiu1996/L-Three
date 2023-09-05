using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Infra.Core.Metadata;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.ViewModels.Messages
{
    public class ImageMessageViewModel : MessageViewModel
    {
        public ImageType ImageType { get; set; }
        public BitmapSource Source { get; set; }
        public string Location { get; set; }
        public long FileId { get; set; }
        public string FileName { get; set; }
        public string RemoteUrl { get; set; }

        public override string GetShortDesc()
        {
            if (ImageType == ImageType.Local)
            {
                return "[图片消息]";
            }
            else
            {
                return "[表情信息]";
            }
        }

        public ImageMessageViewModel() : base(MessageType.Image)
        {
            CopyCommandAsync = new AsyncRelayCommand(CopyAsync);
            LeftButtonClickCommandAsync = new AsyncRelayCommand(LeftButtonClickAsync);
            OpenLocationCommandAsync = new AsyncRelayCommand(OpenLocationAsync);
        }

        public override void FromDto(ChatRecordResponseDto chatRecord)
        {
            base.FromDto(chatRecord);
            ImageType = chatRecord.ImageType;
            if (ImageType == ImageType.Network)
            {
                var source = new BitmapImage();
                try
                {
                    using (MemoryStream ms = new MemoryStream(chatRecord.Bytes))
                    {
                        source.BeginInit();
                        source.StreamSource = ms;
                        source.CacheOption = BitmapCacheOption.OnLoad;
                        source.EndInit();

                        Source = source;
                    }
                }
                finally
                {
                    source.Freeze();
                }
            }
            else
            {
                FileName = chatRecord.FileName;
                var source = new BitmapImage();
                try
                {
                    Location = chatRecord.Message;
                    source.BeginInit();
                    if (string.IsNullOrEmpty(Location))
                    {

                    }
                    source.UriSource = new Uri(Location, UriKind.RelativeOrAbsolute);
                    source.EndInit();

                    Source = source;
                }
                finally
                {
                    source.Freeze();
                }
            }
        }

        public override void ToMessage(FromToMessage fromToMessage)
        {
            base.ToMessage(fromToMessage);
            var message = fromToMessage as ImageMessage;
            message.ImageType = ImageType;
            message.FileId = FileId;
            message.RemoteUrl = RemoteUrl;
        }

        private Task LeftButtonClickAsync()
        {
            if (string.IsNullOrEmpty(Location) || !File.Exists(Location))
            {
                return Task.CompletedTask;
            }
            var imageBrowser = new ImageBrowser(new Uri(Location, UriKind.RelativeOrAbsolute));
            imageBrowser.ResizeMode = System.Windows.ResizeMode.CanResize;
            imageBrowser.Show();
            return Task.CompletedTask;
        }

        private Task CopyAsync()
        {
            if (ImageType == ImageType.Network || string.IsNullOrEmpty(Location) || !File.Exists(Location))
                return Task.CompletedTask;

            var img = new Bitmap(Location);
            System.Windows.Forms.Clipboard.SetImage(img);

            return Task.CompletedTask;
        }

        private Task OpenLocationAsync()
        {
            ExplorerFile(Location);

            return Task.CompletedTask;
        }
    }
}
