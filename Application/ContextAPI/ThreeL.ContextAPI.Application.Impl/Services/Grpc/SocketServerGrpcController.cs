using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using ThreeL.ContextAPI.Application.Contract.Protos;
using ThreeL.ContextAPI.Application.Contract.Services;

namespace ThreeL.ContextAPI.Application.Impl.Services.Grpc
{
    public class SocketServerGrpcController : SocketServerService.SocketServerServiceBase, IAppService
    {
        private readonly IGrpcService _grpcService;
        public SocketServerGrpcController(IGrpcService _grpcService)
        {
            this._grpcService = _grpcService;
        }

        [Authorize]
        public async override Task<SocketServerUserLoginResponse> SocketServerUserLogin(SocketServerUserLoginRequest request, ServerCallContext context)
        {
            try
            {
                return await _grpcService.SocketServerUserLogin(request, context);
            }
            catch(Exception ex)
            {
                return new SocketServerUserLoginResponse() { Result = false };
            }
        }

        [Authorize]
        public async override Task<FileInfoResponse> FetchFileInfo(FileInfoRequest request, ServerCallContext context)
        {
            try
            {
                return await _grpcService.FetchFileInfo(request, context);
            }
            catch
            {
                return new FileInfoResponse() { Result = false };
            }
        }

        [Authorize]
        public async override Task<ChatRecordPostResponse> PostChatRecord(IAsyncStreamReader<ChatRecordPostRequest> requestStream, ServerCallContext context)
        {
            try
            {
                return await _grpcService.PostChatRecord(requestStream, context);
            }
            catch (Exception ex)
            {
                return new ChatRecordPostResponse() { Result = false };
            }
        }

        [Authorize]
        public async override Task<ChatRecordPostResponse> PostChatRecordSingle(ChatRecordPostRequest request, ServerCallContext context)
        {
            try
            {
                return await _grpcService.PostChatRecordSingle(request, context);
            }
            catch (Exception ex)
            {
                return new ChatRecordPostResponse() { Result = false };
            }
        }

        [Authorize]
        public async override Task<ChatRecordWithdrawResponse> WithdrawChatRecord(ChatRecordWithdrawRequest request, ServerCallContext context)
        {
            try
            {
                return await _grpcService.WithdrawChatRecord(request, context);
            }
            catch (Exception ex)
            {
                return new ChatRecordWithdrawResponse() { Result = false };
            }
        }
    }
}
