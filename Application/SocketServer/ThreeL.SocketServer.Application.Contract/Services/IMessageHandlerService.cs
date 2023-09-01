using ThreeL.ContextAPI.Application.Contract.Services;

namespace ThreeL.SocketServer.Application.Contract.Services
{
    public interface IMessageHandlerService : IAppService
    {
        Task<bool> IsValidRelationAsync(long u1, long u2, bool isGroup, string token);
    }
}
