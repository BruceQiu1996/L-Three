using Grpc.Core;
using ThreeL.ContextAPI.Application.Contract.Protos;
using ThreeL.Shared.Application.Contract.Attributes;

namespace ThreeL.ContextAPI.Application.Contract.Services
{
    public interface IGrpcService : IAppService
    {
        [DapperUnitOfWork]
        Task<SocketServerUserLoginResponse> SocketServerUserLogin(SocketServerUserLoginRequest request, ServerCallContext context);

        Task<FileInfoResponse> FetchFileInfo(FileInfoRequest request, ServerCallContext context);

        [DapperUnitOfWork]
        Task<ChatRecordPostResponse> PostChatRecord(IAsyncStreamReader<ChatRecordPostRequest> requestStream, ServerCallContext context);

        [DapperUnitOfWork]
        Task<ChatRecordPostResponse> PostChatRecordSingle(ChatRecordPostRequest request, ServerCallContext context);

        [DapperUnitOfWork]
        Task<VoiceChatRecordPostResponse> PostVoiceChatRecordSingle(VoiceChatRecordPostRequest request, ServerCallContext context);

        [DapperUnitOfWork]
        Task<ChatRecordWithdrawResponse> WithdrawChatRecord(ChatRecordWithdrawRequest request, ServerCallContext context);

        [DapperUnitOfWork]
        Task<AddFriendResponse> AddFriend(AddFriendRequest request, ServerCallContext context);
        [DapperUnitOfWork]
        Task<ReplyAddFriendResponse> ReplyAddFriend(ReplyAddFriendRequest request, ServerCallContext context);
        [DapperUnitOfWork]
        Task<InviteFriendsIntoGroupResponse> InviteFriendsIntoGroup(InviteFriendsIntoGroupRequest request, ServerCallContext context);
        Task<ValidateRelationResponse> ValidateRelation(ValidateRelationRequest request, ServerCallContext context);
        Task<VoiceChatRecorStatusResponse> GetVoiceChatStatus(VoiceChatRecorStatusRequest request, ServerCallContext context);
        [DapperUnitOfWork]
        Task<VoiceChatRecorStatusUpdateResponse> UpdateVoiceChatStatus(VoiceChatRecorStatusUpdateRequest request, ServerCallContext context);
    }
}
