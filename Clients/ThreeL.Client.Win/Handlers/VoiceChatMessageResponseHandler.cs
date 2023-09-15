using CommunityToolkit.Mvvm.Messaging;
using SuperSocket;
using System.Threading.Tasks;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.Handlers
{
    public class VoiceChatMessageResponseHandler : ClientMessageHandler
    {
        public VoiceChatMessageResponseHandler() : base(MessageType.VoiceChat)
        {
        }

        public override Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var packet = message as Packet<VoiceChatMessageResponse>;
            WeakReferenceMessenger.Default.Send<FromToMessageResponse, string>(packet.Body, "message-receive");

            return Task.CompletedTask;
        }
    }
}
