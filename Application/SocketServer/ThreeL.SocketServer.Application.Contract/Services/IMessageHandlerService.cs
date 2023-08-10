namespace ThreeL.SocketServer.Application.Contract.Services
{
    public interface IMessageHandlerService
    {
        Task<bool> IsFriendAsync(long u1, long u2);
    }
}
