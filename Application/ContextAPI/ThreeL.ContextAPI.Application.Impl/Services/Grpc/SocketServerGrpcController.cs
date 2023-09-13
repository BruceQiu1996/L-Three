using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using ThreeL.ContextAPI.Application.Contract.Protos;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.Infra.Core.Metadata;

namespace ThreeL.ContextAPI.Application.Impl.Services.Grpc
{
    public class SocketServerGrpcController : SocketServerService.SocketServerServiceBase, IAppService
    {
        private readonly IGrpcService _grpcService;
        private readonly ILogger _logger;
        public SocketServerGrpcController(IGrpcService grpcService, ILoggerFactory loggerFactory)
        {
            _grpcService = grpcService;
            _logger = loggerFactory.CreateLogger(nameof(Module.CONTEXT_API_GRPC));
        }

        [Authorize]
        public async override Task<SocketServerUserLoginResponse> SocketServerUserLogin(SocketServerUserLoginRequest request, ServerCallContext context)
        {
            try
            {
                return await _grpcService.SocketServerUserLogin(request, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
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
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
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
                _logger.LogError(ex.ToString());
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
                _logger.LogError(ex.ToString());
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
                _logger.LogError(ex.ToString());
                return new ChatRecordWithdrawResponse() { Result = false };
            }
        }

        [Authorize]
        public async override Task<AddFriendResponse> AddFriend(AddFriendRequest request, ServerCallContext context)
        {
            try
            {
                return await _grpcService.AddFriend(request, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new AddFriendResponse() { Result = false, Message = "服务器异常" };
            }
        }

        [Authorize]
        public async override Task<ReplyAddFriendResponse> ReplyAddFriend(ReplyAddFriendRequest request, ServerCallContext context)
        {
            try
            {
                return await _grpcService.ReplyAddFriend(request, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new ReplyAddFriendResponse() { Result = false, Message = "服务器异常" };
            }
        }

        [Authorize]
        public async override Task<InviteFriendsIntoGroupResponse> InviteFriendsIntoGroup(InviteFriendsIntoGroupRequest request, ServerCallContext context)
        {
            try
            {
                return await _grpcService.InviteFriendsIntoGroup(request, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new InviteFriendsIntoGroupResponse() { Result = false, Message = "服务器异常" };
            }
        }

        [Authorize]
        public async override Task<ValidateRelationResponse> ValidateRelation(ValidateRelationRequest request, ServerCallContext context)
        {
            try
            {
                return await _grpcService.ValidateRelation(request, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new ValidateRelationResponse() { Result = false };
            }
        }

        [Authorize]
        public async override Task<VoiceChatRecordPostResponse> PostVoiceChatRecordSingle(VoiceChatRecordPostRequest request, ServerCallContext context)
        {
            try
            {
                return await _grpcService.PostVoiceChatRecordSingle(request, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new VoiceChatRecordPostResponse() { Result = false };
            }
        }

        [Authorize]
        public async override Task<VoiceChatRecorStatusResponse> GetVoiceChatStatus(VoiceChatRecorStatusRequest request, ServerCallContext context)
        {
            try
            {
                return await _grpcService.GetVoiceChatStatus(request, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new VoiceChatRecorStatusResponse() { Started = true };
            }
        }

        [Authorize]
        public async override Task<VoiceChatRecorStatusUpdateResponse> UpdateVoiceChatStatus(VoiceChatRecorStatusUpdateRequest request, ServerCallContext context)
        {
            try
            {
                return await _grpcService.UpdateVoiceChatStatus(request, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new VoiceChatRecorStatusUpdateResponse() { Result = false };
            }
        }
    }
}
