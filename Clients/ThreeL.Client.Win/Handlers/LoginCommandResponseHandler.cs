using SuperSocket;
using System;
using System.Threading.Tasks;
using ThreeL.Shared.SuperSocket.Cache;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Commands;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.Handlers
{
    public class LoginCommandResponseHandler : AbstractMessageHandler
    {
        private readonly PacketWaiter _packetWaiter;
        public LoginCommandResponseHandler(PacketWaiter packetWaiter) : base(MessageType.LoginResponse)
        {
            _packetWaiter = packetWaiter;
        }

        public override Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var packet = message as Packet<LoginCommandResponse>;
            _packetWaiter.AddWaitPacket($"answer:{packet.Sequence}", packet, true);

            return Task.CompletedTask;
        }
    }
}
