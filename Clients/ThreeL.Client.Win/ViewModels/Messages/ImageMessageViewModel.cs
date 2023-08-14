using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Infra.Core.Metadata;

namespace ThreeL.Client.Win.ViewModels.Messages
{
    public class ImageMessageViewModel : MessageViewModel
    {
        public ImageType ImageType { get; set; }
        public BitmapSource Source { get; set; }
        public string Location { get; set; }
        public long FileId { get; set; }

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

        public ImageMessageViewModel()
        {
            CopyCommandAsync = new AsyncRelayCommand(CopyAsync);
            LeftButtonClickCommandAsync = new AsyncRelayCommand(LeftButtonClickAsync);
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
                var source = new BitmapImage();
                try
                {
                    Location = chatRecord.Message;
                    source.BeginInit();
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
            SetFileDrop(Location);

            return Task.CompletedTask;
        }
    }
}
