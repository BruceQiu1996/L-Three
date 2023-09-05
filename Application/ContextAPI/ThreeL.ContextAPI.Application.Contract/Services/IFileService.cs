using Microsoft.AspNetCore.Http;
using ThreeL.ContextAPI.Application.Contract.Dtos.File;

namespace ThreeL.ContextAPI.Application.Contract.Services
{
    public interface IFileService : IAppService
    {
        Task<CheckFileExistResponseDto> CheckFileExistInServerAsync(string code, long userId);
        Task<UploadFileResponseDto> UploadFileAsync(bool isGroup, long userId, long receiver, string code, IFormFile file);
        Task<FileInfo> GetDownloadFileInfoAsync(long userId, string messageId);
    }
}
