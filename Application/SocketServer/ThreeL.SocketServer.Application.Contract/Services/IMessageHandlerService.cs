using ThreeL.ContextAPI.Application.Contract.Services;

namespace ThreeL.SocketServer.Application.Contract.Services
{
    public interface IMessageHandlerService : IAppService
    {
        Task<bool> IsFriendAsync(long u1, long u2);
    }
}
