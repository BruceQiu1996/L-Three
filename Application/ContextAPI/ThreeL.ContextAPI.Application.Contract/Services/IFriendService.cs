using ThreeL.ContextAPI.Application.Contract.Dtos.Relation;

namespace ThreeL.ContextAPI.Application.Contract.Services
{
    public interface IFriendService
    {
        Task<IEnumerable<FriendDto>> GetFriendsAsync(long userId);
    }
}
