using System.Windows.Media.Imaging;
using ThreeL.Client.Shared.Entities.Metadata;

namespace ThreeL.Client.Win.Models
{
    /// <summary>
    /// 表情基类
    /// </summary>
    public class EmojiEntity
    {
        public string Group { get; set; }
        public ImageType ImageType { get; set; }
        public string Url { get; set; }
        public BitmapImage BitmapImage { get; set; }
    }
}
