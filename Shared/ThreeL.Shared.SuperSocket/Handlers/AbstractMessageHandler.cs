using SuperSocket;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Shared.SuperSocket.Handlers
{
    public abstract class AbstractMessageHandler : IMessageHandler
    {
        public bool Enable => true;

        public MessageType MessageType { get; private set; }

        public AbstractMessageHandler(MessageType messageType)
        {
            MessageType = messageType;
        }

        public abstract Task ExcuteAsync(IAppSession appSession, IPacket message);
    }
}
