using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Client.Shared.Services;
using ThreeL.Client.Win.Helpers;
using ThreeL.Client.Win.Pages;
using ThreeL.Client.Win.ViewModels.Messages;

namespace ThreeL.Client.Win.ViewModels
{
    public class FriendViewModel : ObservableObject
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Remark { get; set; }
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
                    RefreshAvatar(Id, value.Value);
                }

                SetProperty(ref avatarId, value);
            }
        }
        public bool IsGroup { get; set; } //是否是群组

        private ObservableCollection<UserRoughlyDto> members;
        public ObservableCollection<UserRoughlyDto> Members
        {
            get => members;
            set => SetProperty(ref members, value);
        }

        public int Port { get; set; }
        public string ShowName => UserName.Substring(0, 1).ToUpper();
        public string DisplayName => string.IsNullOrEmpty(Remark) ? UserName : Remark;
        public List<string> Hosts { get; }
        public bool LoadedChatRecord { get; set; }

        private MessageViewModel lastMessage;
        public MessageViewModel LastMessage
        {
            get => lastMessage;
            set => SetProperty(ref lastMessage, value);
        }

        private string lastMessageShortDesc;
        public string LastMessageShortDesc
        {
            get => lastMessageShortDesc;
            set => SetProperty(ref lastMessageShortDesc, value);
        }

        private ObservableCollection<MessageViewModel> messages;
        public ObservableCollection<MessageViewModel> Messages
        {
            get => messages;
            set => SetProperty(ref messages, value);
        }

        private readonly ContextAPIService _contextAPIService;
        private readonly FileHelper _fileHelper;

        public void AddMessage(MessageViewModel message)
        {
            Messages.Remove(message);
            if (LastMessage == null)
                Messages.Add(new TimeMessageViewModel()
                {
                    DateTime = App.ServiceProvider.GetService<DateTimeHelper>().ConvertDateTimeToText(message.SendTime)
                });

            if (LastMessage != null && LastMessage.SendTime.AddMinutes(5) <= message.SendTime)
                Messages.Add(new TimeMessageViewModel()
                {
                    DateTime = App.ServiceProvider.GetService<DateTimeHelper>().ConvertDateTimeToText(message.SendTime)
                });

            Messages.Add(message);
            message.SendTimeText = App.ServiceProvider.GetService<DateTimeHelper>().ConvertDateTimeToShortText(message.SendTime);
            LastMessage = message;
            LastMessage.ShortDesc = LastMessage.Withdrawed ? "[消息已被撤回]" : LastMessage.GetShortDesc();
            LastMessage.WithdrawMessage = App.UserProfile.UserId == message.From ? "你撤回了一条消息" : "对方撤回了一条消息";
            App.ServiceProvider.GetRequiredService<Chat>().chatScrollViewer.ScrollToEnd();
        }

        public FriendViewModel()
        {
            Hosts = new List<string>();
            Messages = new ObservableCollection<MessageViewModel>();
            Members = new ObservableCollection<UserRoughlyDto>();
        }

        private void RefreshAvatar(long userId, long id)
        {
            var _ = Task.Run(async () =>
            {
                var data = await App.ServiceProvider.GetRequiredService<ContextAPIService>().DownloadUserAvatarAsync(userId, id);
                if (data != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Avatar = App.ServiceProvider.GetRequiredService<FileHelper>().ByteArrayToBitmapImage(data);
                    });
                }
            });
        }
    }
}
