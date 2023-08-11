using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows.Media.Imaging;
using ThreeL.Client.Shared.Entities;
using ThreeL.Client.Shared.Entities.Metadata;
using ThreeL.Client.Win.Helpers;

namespace ThreeL.Client.Win.ViewModels.Messages
{
    public class ImageMessageViewModel : MessageViewModel
    {
        public ImageType ImageType { get; set; }
        public string Url { get; set; }
        public BitmapImage Source { get; set; }

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

        public override void FromEntity(ChatRecord chatRecord)
        {
            base.FromEntity(chatRecord);
            ImageType = chatRecord.ImageType;
            if (ImageType == ImageType.Network)
            {
                Url = chatRecord.ResourceLocalLocation;
            }
            else 
            {
                if (File.Exists(chatRecord.ResourceLocalLocation))
                {
                    Source = App.ServiceProvider.GetRequiredService<FileHelper>().ByteArrayToBitmapImage(
                         File.ReadAllBytes(chatRecord.ResourceLocalLocation));
                }
                else 
                {
                    //TODO 服务器查找
                }
            }
        }
    }
}
