using System.Windows.Media.Imaging;
using ThreeL.Infra.Core.Metadata;

namespace ThreeL.Client.Win.Models
{
    /// <summary>
    /// 表情基类
    /// </summary>
    public class EmojiEntity
    {
        public string Url { get; set; }
        public BitmapImage BitmapImage { get; set; }
    }
}
