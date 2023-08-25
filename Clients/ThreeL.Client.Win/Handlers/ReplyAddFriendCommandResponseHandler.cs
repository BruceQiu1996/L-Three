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
    public class ReplyAddFriendCommandResponseHandler : AbstractMessageHandler
    {
        private readonly GrowlHelper _grolHelper;
        public ReplyAddFriendCommandResponseHandler(GrowlHelper grolHelper) : base(MessageType.ReplyAddFriendResponse)
        {
            _grolHelper = grolHelper;
        }

        public override Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var packet = message as Packet<ReplyAddFriendCommandResponse>;
            if (!packet.Body.Result && packet.Body.To == App.UserProfile.UserId)
            {
                _grolHelper.Warning($"操作失败:{packet.Body.Message}");
            }

            if (packet.Body.Result)
            {
                WeakReferenceMessenger.Default.Send<ReplyAddFriendCommandResponse, string>(packet.Body, "message-addfriend-success");
            }

            return Task.CompletedTask;
        }
    }
}
