using ThreeL.ContextAPI.Application.Contract.Services;
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
        [GrpcException]
        Task<AddFriendResponse> AddFriendAsync(AddFriendRequest request, string token);
        [GrpcException]
        Task<ReplyAddFriendResponse> ReplyAddFriendAsync(ReplyAddFriendRequest request, string token);
        [GrpcException]
        Task<InviteFriendsIntoGroupResponse> InviteFriendsIntoGroupAsync(InviteFriendsIntoGroupRequest request, string token);
        [GrpcException]
        Task<ValidateRelationResponse> ValidateRelation(ValidateRelationRequest request, string token);
        [GrpcException]
        Task<VoiceChatRecordPostResponse> PostVoiceChatRecordAsync(VoiceChatRecordPostRequest request, string token);
        [GrpcException]
        Task<VoiceChatRecorStatusResponse> GetVoiceChatStatus(VoiceChatRecorStatusRequest request, string token);
        [GrpcException]
        Task<VoiceChatRecorStatusUpdateResponse> UpdateVoiceChatStatus(VoiceChatRecorStatusUpdateRequest request, string token);
        [GrpcException]
        Task<VoiceChatRecorStatusUpdateResponse> FinishVoiceChat(VoiceChatRecordFinishRequest request, string token);
    }
}
