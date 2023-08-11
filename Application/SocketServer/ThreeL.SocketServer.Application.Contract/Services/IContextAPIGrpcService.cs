namespace ThreeL.SocketServer.Application.Contract.Services
{
    public interface IContextAPIGrpcService
    {
        void SetToken(string token);
        Task<SocketServerUserLoginResponse> SocketServerUserLoginAsync(SocketServerUserLoginRequest request);
        Task<FileInfoResponse> FetchFileInfoAsync(FileInfoRequest request);
    }
}
