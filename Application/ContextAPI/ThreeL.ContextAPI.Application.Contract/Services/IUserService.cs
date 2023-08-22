using Microsoft.AspNetCore.Http;
using ThreeL.ContextAPI.Application.Contract.Dtos.File;
using ThreeL.ContextAPI.Application.Contract.Dtos.User;
using ThreeL.Shared.Application.Contract.Services;

namespace ThreeL.ContextAPI.Application.Contract.Services
{
    public interface IUserService : IAppService
    {
        Task<ServiceResult> CreateUserAsync(UserCreationDto creationDto, long creator);
        Task<UserLoginResponseDto> LoginAsync(UserLoginDto userLoginDto);
        Task<bool> LoginByCodeAsync(UserLoginDto userLoginDto);
        Task<UserRefreshTokenDto> RefreshAuthTokenAsync(UserRefreshTokenDto token);
        Task<ServiceResult<IEnumerable<UserRoughlyDto>>> FindUserByKeyword(long userId, string keyword);
        Task<CheckFileExistResponseDto> CheckAvatarExistInServerAsync(string code, long userId);
        Task<ServiceResult<UserUpdateAvatarResponseDto>> UpdateUserAvatarAsync(UserUpdateAvatarDto userUpdateDto, long userId);
        Task<ServiceResult<UploadFileResponseDto>> UploadUserAvatarAsync(long userId, string code, IFormFile file);
    }
}
