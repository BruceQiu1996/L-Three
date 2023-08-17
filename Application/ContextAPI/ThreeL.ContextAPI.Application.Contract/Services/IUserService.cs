using ThreeL.ContextAPI.Application.Contract.Dtos.User;

namespace ThreeL.ContextAPI.Application.Contract.Services
{
    public interface IUserService : IAppService
    {
        Task CreateUserAsync(UserCreationDto creationDto, long creator);
        Task<UserLoginResponseDto> LoginAsync(UserLoginDto userLoginDto);
        Task<bool> LoginByCodeAsync(UserLoginDto userLoginDto);
        Task<UserRefreshTokenDto> RefreshAuthTokenAsync(UserRefreshTokenDto token);
    }
}
