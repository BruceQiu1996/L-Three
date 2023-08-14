using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Specialized;
using System.Windows;
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
        private bool _sendSuccess = true;
        public bool SendSuccess
        {
            get => _sendSuccess;
            set => SetProperty(ref _sendSuccess, value);
        }

        private bool _sending = false;
        public bool Sending
        {
            get => _sending;
            set => SetProperty(ref _sending, value);
        }
        public AsyncRelayCommand CopyCommandAsync { get; set; }
        public AsyncRelayCommand WithDrawCommandAsync { get; set; }
        public AsyncRelayCommand LeftButtonClickCommandAsync { get; set; }

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

        protected void SetFileDrop(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            SetFileDropList(new[] { filePath });
        }

        private void SetFileDropList(string[] files)
        {
            Clipboard.Clear();//清空剪切板 
            StringCollection strcoll = new StringCollection();
            foreach (var file in files)
            {
                strcoll.Add(file);
            }
            Clipboard.SetFileDropList(strcoll);
        }
    }
}
