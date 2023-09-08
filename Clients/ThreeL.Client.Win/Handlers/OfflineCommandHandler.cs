using CommunityToolkit.Mvvm.Messaging;
using SuperSocket;
using System.Threading.Tasks;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Commands;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.Handlers
{
    public class OfflineCommandHandler : AbstractMessageHandler
    {
        public OfflineCommandHandler() : base(MessageType.RequestOffline) { }

        public override Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var packet = message as Packet<OfflineCommand>;
            WeakReferenceMessenger.Default.Send(packet.Body.Reason, "message-request-offline");

            return Task.CompletedTask;
        }
    }
}
