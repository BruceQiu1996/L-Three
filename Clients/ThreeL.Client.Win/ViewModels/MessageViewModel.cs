using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using ThreeL.Client.Shared.Dtos.ContextAPI;

namespace ThreeL.Client.Win.ViewModels
{
    public class MessageViewModel : ObservableObject
    {
        public string MessageId { get; set; }
        public DateTime SendTime { get; set; }
        public bool FromSelf { get; set; }
        public long From { get; set; }
        public long To { get; set; }

        public AsyncRelayCommand CopyCommandAsync { get; set; }
        public AsyncRelayCommand WithDrawCommandAsync { get; set; }

        public MessageViewModel()
        {

        }

        public virtual void FromDto(ChatRecordResponseDto chatRecord)
        {
            MessageId = chatRecord.MessageId;
            From = chatRecord.From;
            To = chatRecord.To;
            SendTime = chatRecord.SendTime;
            FromSelf = App.UserProfile == null ? true : App.UserProfile.UserId == From ? true : false;
        }

        public virtual string GetShortDesc()
        {
            return "[消息]";
        }
    }
}
