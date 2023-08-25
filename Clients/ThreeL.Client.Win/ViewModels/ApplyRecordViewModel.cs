using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Client.Shared.Utils;
using ThreeL.Client.Win.Helpers;
using ThreeL.Infra.Core.Enum;
using ThreeL.Shared.SuperSocket.Client;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Commands;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.ViewModels
{
    public class ApplyRecordViewModel : ObservableObject
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public bool FromSelf { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ProcessTime { get; set; }
        public FriendApplyStatus Status { get; set; }
        public string StatusText { get; set; }
        public string Content { get; set; }
        public string CreateTimeText { get; set; }
        public bool Processed { get; set; }

        public AsyncRelayCommand AgreeCommandAsync { get; set; }
        public AsyncRelayCommand RejectCommandAsync { get; set; }

        public ApplyRecordViewModel(FriendApplyResponseDto responseDto)
        {
            Id = responseDto.Id;
            UserName = responseDto.ActiverId == App.UserProfile.UserId ? responseDto.PassiverName : responseDto.ActiverName;
            FromSelf = responseDto.ActiverId == App.UserProfile.UserId;
            CreateTime = responseDto.CreateTime;
            ProcessTime = responseDto.ProcessTime;
            Status = responseDto.Status;

            CreateTimeText = App.ServiceProvider.GetRequiredService<DateTimeHelper>().ConvertDateTimeToShortText(responseDto.CreateTime);
            Content = FromSelf ? $"添加[{UserName}]为好友" : $"[{UserName}]申请添加你为好友";
            Processed = Status != FriendApplyStatus.TobeProcessed;
            StatusText = Status.GetDescription();

            AgreeCommandAsync = new AsyncRelayCommand(AgreeAsync);
            RejectCommandAsync = new AsyncRelayCommand(RejectAsync);
        }

        private async Task AgreeAsync()
        {
            await SendReply(true);
        }

        private async Task RejectAsync()
        {
            await SendReply(false);
        }

        private async Task SendReply(bool agree)
        {
            if (Status != FriendApplyStatus.TobeProcessed) return;
            if (FromSelf) return;

            var packet = new Packet<ReplyAddFriendCommand>()
            {
                Sequence = App.ServiceProvider.GetRequiredService<SequenceIncrementer>().GetNextSequence(),
                MessageType = MessageType.ReplyAddFriend,
                Body = new ReplyAddFriendCommand
                {
                    ApplyId = Id,
                    Agree = agree
                }
            };

            var sendResult = await App.ServiceProvider.GetRequiredService<TcpSuperSocketClient>().SendBytesAsync(packet.Serialize());
            if (!sendResult)
            {
                App.ServiceProvider.GetRequiredService<GrowlHelper>().Warning("操作失败!请稍后再试");
            }
        }
    }
}