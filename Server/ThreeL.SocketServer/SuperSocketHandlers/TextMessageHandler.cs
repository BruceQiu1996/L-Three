using SuperSocket;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.SocketServer.SuperSocketHandlers
{
    public class TextMessageHandler : AbstractMessageHandler
    {
        public TextMessageHandler() : base(MessageType.Text)
        {

        }

        public override Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var packet = message as Packet<TextMessage>;
            Console.WriteLine(packet?.Body?.Text);
            return Task.CompletedTask;
        }
    }
}
