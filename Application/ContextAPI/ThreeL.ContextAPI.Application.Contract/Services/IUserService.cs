using ThreeL.ContextAPI.Application.Contract.Dtos.User;

namespace ThreeL.ContextAPI.Application.Contract.Services
{
    public interface IUserService
    {
        Task CreateUserAsync(UserCreationDto creationDto, long creator);
        Task<UserLoginResponseDto> LoginAsync(UserLoginDto userLoginDto);
    }
}
