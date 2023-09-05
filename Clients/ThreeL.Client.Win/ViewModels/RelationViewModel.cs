using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Client.Shared.Services;
using ThreeL.Client.Win.Helpers;
using ThreeL.Client.Win.Pages;
using ThreeL.Client.Win.ViewModels.Messages;

namespace ThreeL.Client.Win.ViewModels
{
    public class RelationViewModel : ObservableObject
    {
        public long Id { get; set; }
        public string Name { get; set; }
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
                    App.ServiceProvider.GetRequiredService<FileHelper>().RefreshAvatarAsync(Id, value.Value, source => Avatar = source);
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
        public string ShowName => Name.Substring(0, 1).ToUpper();
        public string TitleDisplayName { get; set; }
        public string DisplayName => string.IsNullOrEmpty(Remark) ? Name : Remark;
        public List<string> Hosts { get; }
        public bool LoadedChatRecord { get; set; }
        //TODO暂时只是在线的未读消息数量，后续需要智能，并且各端同步
        private int unReadCount;
        public int UnReadCount
        {
            get => unReadCount;
            set=>SetProperty(ref unReadCount, value);
        }

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
            var temp = Messages.FirstOrDefault(x => x.MessageId == message.MessageId);
            if (temp != null)
            {
                Messages.Remove(temp);
            }
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
            LastMessage = message;
            LastMessage.ShortDesc = LastMessage.Withdrawed ? "[消息已被撤回]" : LastMessage.GetShortDesc();
            App.ServiceProvider.GetRequiredService<Chat>().chatScrollViewer.ScrollToEnd();
        }

        public void AddMessages(IEnumerable<MessageViewModel> messages, bool before = false)
        {
            if (messages == null || messages.Count() <= 0)
                return;
            if (before)
            {
                var temp = Messages.FirstOrDefault();
                if (temp != null && temp is LoadRecordViewModel)
                {
                    Messages.Remove(temp);
                }
            }
            foreach (var message in messages)
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

                if (before)
                {
                    Messages.Insert(0, message);
                }
                else
                {
                    Messages.Add(message);
                    LastMessage = message;
                    LastMessage.ShortDesc = LastMessage.Withdrawed ? "[消息已被撤回]" : LastMessage.GetShortDesc();
                }
            }

            if (Messages.FirstOrDefault() is not LoadRecordViewModel) 
            {
                Messages.Insert(0, new LoadRecordViewModel() { Relation = this});
            }

            App.ServiceProvider.GetRequiredService<Chat>().chatScrollViewer.ScrollToEnd();
        }

        public RelationViewModel()
        {
            Hosts = new List<string>();
            Messages = new ObservableCollection<MessageViewModel>();
            Members = new ObservableCollection<UserRoughlyDto>();
        }
    }
}
