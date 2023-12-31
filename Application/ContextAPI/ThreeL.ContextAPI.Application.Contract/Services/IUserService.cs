﻿using Microsoft.AspNetCore.Http;
using ThreeL.ContextAPI.Application.Contract.Dtos.User;
using ThreeL.Shared.Application.Contract.Attributes;
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
        [DapperUnitOfWork]
        Task<ServiceResult<UserAvatarCheckExistResponseDto>> CheckAvatarExistInServerAsync(string code, long userId);
        [DapperUnitOfWork]
        Task<ServiceResult<FileInfo>> UploadUserAvatarAsync(long userId, string code, IFormFile file);
        Task<ServiceResult<FileInfo>> DownloadUserAvatarAsync(long avatarId, long userId);
        [DapperUnitOfWork]
        Task<ServiceResult<GroupCreationResponseDto>> CreateGroupChatAsync(long userId, string groupName);
        [DapperUnitOfWork]
        Task<ServiceResult> InviteUserJoinGroupChatAsync(long userId, long groupId, IEnumerable<long> ids);
        Task<ServiceResult<UserRoughlyDto>> FetchUserInfoByIdAsync(long userId, long sUserId);
        Task<ServiceResult<GroupRoughlyDto>> FetchGroupInfoByIdAsync(long groupId);
    }
}
