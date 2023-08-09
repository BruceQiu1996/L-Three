using System.Windows.Media.Imaging;
using ThreeL.Client.Shared.Entities.Metadata;

namespace ThreeL.Client.Win.ViewModels.Messages
{
    public class ImageMessage : MessageViewModel
    {
        public ImageType ImageType { get; set; }
        public string Url { get; set; }
        public BitmapImage Source { get; set; }
    }
}
