using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Media.Imaging;
using ThreeL.Client.Win.Helpers;

namespace ThreeL.Client.Win.ViewModels
{
    public class DetailWindowViewModel : ObservableObject
    {
        public long Id { get; set; }

        private BitmapImage _avatar;
        public BitmapImage Avatar
        {
            get { return _avatar; }
            set => SetProperty(ref _avatar, value);
        }

        private long? avatarId;
        public long? AvatarId
        {
            get => avatarId;
            set
            {
                if (value != null && value != avatarId)
                {
                    App.ServiceProvider.GetRequiredService<FileHelper>().RefreshAvatarAsync(Id, value.Value);
                }

                avatarId = value;
            }
        }
    }
}
