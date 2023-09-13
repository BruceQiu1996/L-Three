using CommunityToolkit.Mvvm.Messaging;
using SuperSocket;
using System.Threading.Tasks;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.Handlers
{
    public class VoiceChatEventResponseHandler : AbstractMessageHandler
    {
        public VoiceChatEventResponseHandler() : base(MessageType.VoiceChatEventResponse)
        {
        }

        public override Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var packet = message as Packet<VoiceChatStatusResponse>;
            WeakReferenceMessenger.Default.Send<VoiceChatStatusResponse, string>(packet.Body, "message-receive-voicechat-event");

            return Task.CompletedTask;
        }
    }
}
