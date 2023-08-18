﻿using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.SocketServer.Application.Contract.Interceptors;

namespace ThreeL.SocketServer.Application.Contract.Services
{
    public interface IContextAPIGrpcService : IAppService
    {
        [GrpcException]
        Task<SocketServerUserLoginResponse> SocketServerUserLoginAsync(SocketServerUserLoginRequest request, string token);
        [GrpcException]
        Task<FileInfoResponse> FetchFileInfoAsync(FileInfoRequest request, string token);
        [GrpcException]
        Task<ChatRecordPostResponse> PostChatRecordsAsync(IEnumerable<ChatRecordPostRequest> request, string token);
        [GrpcException]
        Task<ChatRecordPostResponse> PostChatRecordAsync(ChatRecordPostRequest request, string token);
        [GrpcException]
        Task<ChatRecordWithdrawResponse> WithdrawChatRecordAsync(ChatRecordWithdrawRequest request, string token);
    }
}
