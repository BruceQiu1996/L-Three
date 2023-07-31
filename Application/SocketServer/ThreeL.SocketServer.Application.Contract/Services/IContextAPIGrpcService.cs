namespace ThreeL.SocketServer.Application.Contract.Services
{
    public interface IContextAPIGrpcService
    {
        Task<SocketServerUserLoginResponse> SocketServerUserLoginAsync(SocketServerUserLoginRequest request);
    }
}
