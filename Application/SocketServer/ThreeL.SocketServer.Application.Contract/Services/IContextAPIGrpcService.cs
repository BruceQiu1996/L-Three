namespace ThreeL.SocketServer.Application.Contract.Services
{
    public interface IContextAPIGrpcService
    {
        Task<SocketServerUserLoginResponse> SocketServerUserLoginAsync(SocketServerUserLoginRequest request, string token);
        Task<FileInfoResponse> FetchFileInfoAsync(FileInfoRequest request, string token);
        Task<ChatRecordPostResponse> PostChatRecordsAsync(IEnumerable<ChatRecordPostRequest> request, string token);
        Task<ChatRecordPostResponse> PostChatRecordAsync(ChatRecordPostRequest request, string token);
        Task<ChatRecordWithdrawResponse> WithdrawChatRecordAsync(ChatRecordWithdrawRequest request, string token);
    }
}
