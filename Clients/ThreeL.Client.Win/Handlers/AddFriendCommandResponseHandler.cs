using CommunityToolkit.Mvvm.Messaging;
using SuperSocket;
using System.Threading.Tasks;
using ThreeL.Client.Win.Helpers;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Commands;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.Handlers
{
    public class AddFriendCommandResponseHandler : AbstractMessageHandler
    {
        private readonly GrowlHelper _grolHelper;
        public AddFriendCommandResponseHandler(GrowlHelper grolHelper) : base(MessageType.AddFriendResponse)
        {
            _grolHelper = grolHelper;
        }

        public override Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var packet = message as Packet<AddFriendCommandResponse>;
            if (!packet.Body.Result && packet.Body.From == App.UserProfile.UserId)
            {
                _grolHelper.Warning($"发送添加好友消息失败:{packet.Body.Message}");

                return Task.CompletedTask;
            }

            if (packet.Body.Result && packet.Body.From == App.UserProfile.UserId)
            {
                _grolHelper.Success($"发送添加好友消息成功");

                return Task.CompletedTask;
            }

            if (packet.Body.Result)
            {
                WeakReferenceMessenger.Default.Send<string, string>(string.Empty, "message-addfriend-apply");

                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    }
}
