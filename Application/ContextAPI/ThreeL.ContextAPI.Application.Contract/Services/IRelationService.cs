using ThreeL.ContextAPI.Application.Contract.Dtos.Relation;

namespace ThreeL.ContextAPI.Application.Contract.Services
{
    public interface IRelationService : IAppService
    {
        Task<IEnumerable<RelationDto>> GetRelationsAndChatRecordsAsync(long userId, DateTime dateTime);
        Task<bool> IsFriendAsync(long userId, long fUserId);
        Task<IEnumerable<FriendApplyResponseDto>> FetchAllFriendApplysAsync(long userId);
    }
}
