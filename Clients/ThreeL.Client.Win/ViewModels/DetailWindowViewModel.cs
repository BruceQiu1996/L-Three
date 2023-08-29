using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Media.Imaging;
using ThreeL.Client.Win.Helpers;

namespace ThreeL.Client.Win.ViewModels
{
    public class DetailWindowViewModel : ObservableObject
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateTimeText => App.ServiceProvider.GetService<DateTimeHelper>().ConvertDateTimeToText(CreateTime);
        public string ShowName => Name.Substring(0, 1).ToUpper();

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
                    App.ServiceProvider.GetRequiredService<FileHelper>().RefreshAvatarAsync(Id, value.Value, source => Avatar = source);
                }

                avatarId = value;
            }
        }
    }
}
