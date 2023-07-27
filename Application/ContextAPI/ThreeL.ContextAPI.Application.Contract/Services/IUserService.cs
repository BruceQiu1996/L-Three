using ThreeL.ContextAPI.Application.Contract.Dtos.User;
using ThreeL.Shared.Application.Contract.Attributes;

namespace ThreeL.ContextAPI.Application.Contract.Services
{
    public interface IUserService
    {
        [DapperUnitOfWork]
        Task TestAsync();
        Task CreateUserAsync(UserCreationDto creationDto, long creator);
        Task<UserLoginResponseDto> LoginAsync(UserLoginDto userLoginDto);
        Task<UserRefreshTokenDto> RefreshAuthTokenAsync(UserRefreshTokenDto token);
    }
}
