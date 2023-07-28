namespace ThreeL.SocketServer.Application.Contract.Services
{
    public interface IContextAPIGrpcService
    {
        Task SocketServerUserLoginAsync(SocketServerUserLoginRequest request);
    }
}
