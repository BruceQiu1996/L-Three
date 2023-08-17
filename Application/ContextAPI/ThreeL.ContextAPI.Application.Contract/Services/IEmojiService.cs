using ThreeL.ContextAPI.Application.Contract.Dtos.Emoji;

namespace ThreeL.ContextAPI.Application.Contract.Services
{
    public interface IEmojiService : IAppService
    {
        Task<EmojiResponseDto> GetEmojiGroupsAsync(string endpoint, string applicationFolder);
    }
}
