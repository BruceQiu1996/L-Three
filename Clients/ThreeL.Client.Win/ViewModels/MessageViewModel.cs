using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Client.Win.Helpers;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.ViewModels
{
    public class MessageViewModel : ObservableObject
    {
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        public MessageType MessageType { get; set; }
        public DateTime SendTime { get; set; }
        public bool FromSelf => App.UserProfile == null ? true : App.UserProfile.UserId == From ? true : false;
        public long From { get; set; }
        public long To { get; set; }

        private bool _sendSuccess = true;
        public bool SendSuccess
        {
            get => _sendSuccess;
            set
            {
                SetProperty(ref _sendSuccess, value);
                CanWithdraw = !Withdrawed && !Sending && SendSuccess && FromSelf;
            }
        }

        private bool _sending = false;
        public bool Sending
        {
            get => _sending;
            set => SetProperty(ref _sending, value);
        }
        public string SendTimeText => App.ServiceProvider.GetService<DateTimeHelper>().ConvertDateTimeToShortText(SendTime);
        public bool CanCopy { get; protected set; } = true;
        public bool CanOpenLocation { get; protected set; } = true;

        private bool _canWithdraw;
        public bool CanWithdraw
        {
            get => !Withdrawed && !Sending && SendSuccess && FromSelf;
            set => SetProperty(ref _canWithdraw, value);
        }

        private bool _withdrawed;
        public bool Withdrawed
        {
            get => _withdrawed;
            set
            {
                SetProperty(ref _withdrawed, value);
                CanWithdraw = !Withdrawed && !Sending && SendSuccess && FromSelf;
                if (value) 
                {
                    ShortDesc = "[消息已被撤回]";
                }
            }
        }

        public bool IsGroup { get; set; }
        public string WithdrawMessage { get; set; } //撤回消息后的显示

        private string _shortDesc;
        public string ShortDesc
        {
            get => _shortDesc;
            set
            {
                SetProperty(ref _shortDesc, value);
            }
        }

        public AsyncRelayCommand CopyCommandAsync { get; set; }
        public AsyncRelayCommand WithDrawCommandAsync { get; set; }
        public AsyncRelayCommand LeftButtonClickCommandAsync { get; set; }
        public AsyncRelayCommand OpenLocationCommandAsync { get; set; }
        public AsyncRelayCommand ReSendWhenFaildCommandAsync { get; set; }
        public AsyncRelayCommand WithdrawCommandAsync { get; set; }

        public MessageViewModel(MessageType messageType)
        {
            MessageType = messageType;
            ReSendWhenFaildCommandAsync = new AsyncRelayCommand(ReSendWhenFaildAsync);
            WithdrawCommandAsync = new AsyncRelayCommand(WithdrawAsync);
        }

        /// <summary>
        /// 从Dto转为vm
        /// </summary>
        /// <param name="chatRecord"></param>
        public virtual void FromDto(ChatRecordResponseDto chatRecord)
        {
            MessageId = chatRecord.MessageId;
            From = chatRecord.From;
            To = chatRecord.To;
            IsGroup = chatRecord.IsGroup;
            SendTime = chatRecord.SendTime;
        }

        /// <summary>
        /// 从vm转为message
        /// </summary>
        /// <param name="fromToMessage"></param>
        public virtual void ToMessage(FromToMessage fromToMessage)
        {
            fromToMessage.MessageId = MessageId;
            fromToMessage.To = To;
            fromToMessage.IsGroup = IsGroup;
            fromToMessage.SendTime = SendTime;
        }

        public virtual string GetShortDesc()
        {
            return "[消息]";
        }

        public virtual Task ReSendWhenFaildAsync()
        {
            //发送中或者发送成功的消息无法重试
            if (Sending || SendSuccess)
            { return Task.CompletedTask; }

            WeakReferenceMessenger.Default.Send(this, "message-resend");

            return Task.CompletedTask;
        }

        public virtual Task WithdrawAsync()
        {
            //发送中或者发送失败的消息无法撤回
            if (Sending || !SendSuccess)
            { return Task.CompletedTask; }

            //if (SendTime.AddMinutes(2) < DateTime.Now)
            //{
            //    App.ServiceProvider.GetRequiredService<GrowlHelper>().Warning("超过两分钟的消息无法撤回");
            //    return Task.CompletedTask;
            //}

            WeakReferenceMessenger.Default.Send(this, "message-withdraw");

            return Task.CompletedTask;
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

        protected void ExplorerFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            FileInfo fileInfo = new FileInfo(filePath);
            Process.Start("Explorer", "/select," + fileInfo.Directory.FullName + "\\" + fileInfo.Name);
        }
    }
}
