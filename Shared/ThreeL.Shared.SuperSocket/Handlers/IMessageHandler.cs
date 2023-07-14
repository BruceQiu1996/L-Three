using SuperSocket;
using ThreeL.Shared.SuperSocket.Dto;

namespace ThreeL.Shared.SuperSocket.Handlers
{
    public interface IMessageHandler
    {
        bool Enable { get; }
        string Name { get; }
        Task ExcuteAsync(IAppSession appSession, IPacket message);
    }
}
