using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using ThreeL.Client.Shared.Services;
using ThreeL.Client.Shared.Utils;
using ThreeL.Client.Win.Helpers;
using ThreeL.Shared.SuperSocket.Client;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Commands;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.ViewModels
{
    public class UserApplyViewModel : ObservableObject
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string ShowName => UserName.Substring(0, 1).ToUpper();

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

        public string Sign { get; set; }
        public bool IsFriend { get; set; } //已经建立好友关系
        public string Role { get; set; }

        public AsyncRelayCommand AddFriendCommandAsync { get; set; }
        public UserApplyViewModel()
        {
            AddFriendCommandAsync = new AsyncRelayCommand(AddFriendAsync);
        }

        private async Task AddFriendAsync()
        {
            if (IsFriend)
            {
                return;
            }

            var packet = new Packet<AddFriendCommand>()
            {
                Sequence = App.ServiceProvider.GetRequiredService<SequenceIncrementer>().GetNextSequence(),
                MessageType = MessageType.AddFriend,
                Body = new AddFriendCommand
                {
                    FriendId = Id
                }
            };

            var sendResult = await App.ServiceProvider.GetRequiredService<TcpSuperSocketClient>().SendBytesAsync(packet.Serialize());
            if (!sendResult)
            {
                App.ServiceProvider.GetRequiredService<GrowlHelper>().Warning("添加好友失败!请稍后再试");
            }
        }
    }
}
