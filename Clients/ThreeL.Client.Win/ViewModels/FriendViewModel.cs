using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ThreeL.Client.Win.Helpers;
using ThreeL.Client.Win.ViewModels.Messages;

namespace ThreeL.Client.Win.ViewModels
{
    public class FriendViewModel : ObservableObject
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Remark { get; set; }
        public string Avatar { get; set; }
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

        public void AddMessage(MessageViewModel message)
        {
            if (LastMessage == null)
                Messages.Add(new TimeMessageViewModel()
                {
                    DateTime = App.ServiceProvider.GetService<DateTimeHelper>().ConvertDateTimeToText(DateTime.Now)
                });

            if (LastMessage != null && LastMessage.SendTime.AddMinutes(5) <= message.SendTime)
                Messages.Add(new TimeMessageViewModel()
                {
                    DateTime = App.ServiceProvider.GetService<DateTimeHelper>().ConvertDateTimeToText(DateTime.Now)
                });

            Messages?.Add(message);
            LastMessage = message;
            LastMessageShortDesc = message.GetShortDesc();
        }

        public FriendViewModel()
        {
            Hosts = new List<string>();
            Messages = new ObservableCollection<MessageViewModel>();
        }
    }
}
