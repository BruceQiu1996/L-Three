using ThreeL.ContextAPI.Domain.Aggregates.UserAggregate;

namespace ThreeL.ContextAPI.Application.Contract.Services
{
    public interface IUserService
    {
        Task<User> SearchAllUsersAsync();
    }
}
