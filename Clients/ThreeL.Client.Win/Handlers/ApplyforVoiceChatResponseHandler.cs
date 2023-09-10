using CommunityToolkit.Mvvm.Messaging;
using SuperSocket;
using System.Threading.Tasks;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.Handlers
{
    public class ApplyforVoiceChatResponseHandler : AbstractMessageHandler
    {
        public ApplyforVoiceChatResponseHandler() : base(MessageType.ApplyVoiceChatResponse)
        {
        }

        public override Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var packet = message as Packet<ApplyforVoiceChatMessageResponse>;
            WeakReferenceMessenger.Default.Send<ApplyforVoiceChatMessageResponse, string>(packet.Body, "message-receive-voice-request");

            return Task.CompletedTask;
        }
    }
}
