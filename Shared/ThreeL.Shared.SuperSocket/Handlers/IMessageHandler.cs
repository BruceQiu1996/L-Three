using SuperSocket;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Shared.SuperSocket.Handlers
{
    public interface IMessageHandler
    {
        bool Enable { get; }
        MessageType MessageType { get; }
        Task ExcuteAsync(IAppSession appSession, IPacket message);
    }
}
