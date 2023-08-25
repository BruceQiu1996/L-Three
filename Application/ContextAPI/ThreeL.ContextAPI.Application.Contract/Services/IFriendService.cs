using ThreeL.ContextAPI.Application.Contract.Dtos.Relation;

namespace ThreeL.ContextAPI.Application.Contract.Services
{
    public interface IFriendService : IAppService
    {
        Task<IEnumerable<FriendDto>> GetFriendsAsync(long userId);
        Task<bool> IsFriendAsync(long userId, long fUserId);
        Task<FriendChatRecordResponseDto> FetchChatRecordsWithFriendAsync(long sender, long receiver, DateTime dateTime);
        Task<IEnumerable<FriendApplyResponseDto>> FetchAllFriendApplysAsync(long userId);
    }
}
