using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Infra.Core.Metadata;

namespace ThreeL.Client.Win.ViewModels.Messages
{
    public class ImageMessageViewModel : MessageViewModel
    {
        public ImageType ImageType { get; set; }
        public string Url { get; set; }
        public BitmapImage Source { get; set; }
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

        }

        public override void FromDto(ChatRecordResponseDto chatRecord)
        {
            base.FromDto(chatRecord);
            ImageType = chatRecord.ImageType;
            if (ImageType == ImageType.Network)
            {
                Url = chatRecord.Message;
            }
            else
            {
                var source = new BitmapImage();
                try
                {
                    string imgUrl = chatRecord.Message;
                    source.BeginInit();
                    source.UriSource = new Uri(imgUrl, UriKind.RelativeOrAbsolute);
                    source.EndInit();

                    Source = source;
                }
                finally
                {
                    source.Freeze();
                }
            }
        }
    }
}
