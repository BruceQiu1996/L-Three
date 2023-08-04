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
        public string DisplayName => string.IsNullOrEmpty(Remark) ? UserName : Remark;
        public List<string> Hosts { get;  }

        private ObservableCollection<MessageViewModel> messages;
        public ObservableCollection<MessageViewModel> Messages
        {
            get => messages;
            set => SetProperty(ref messages, value);
        }

        public FriendViewModel()
        {
            Hosts = new List<string>();
            Messages = new ObservableCollection<MessageViewModel>
            {
                new TimeMessage()
                {
                    DateTime = App.ServiceProvider.GetService<DateTimeHelper>().ConvertDateTimeToText(DateTime.Now)
                }
            };
        }
    }
}
