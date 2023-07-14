using SuperSocket;
using ThreeL.Shared.SuperSocket.Dto;

namespace ThreeL.Shared.SuperSocket.Handlers
{
    public abstract class AbstractMessageHandler : IMessageHandler
    {
        public bool Enable => true;

        public string Name { get; private set; }

        public AbstractMessageHandler(string name)
        {
            Name = name;
        }

        public abstract Task ExcuteAsync(IAppSession appSession, IPacket message);
    }
}
