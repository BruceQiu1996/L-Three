using ThreeL.ContextAPI.Application.Contract.Dtos.Emoji;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.ContextAPI.Domain.Aggregates.EmojiAggregate;
using ThreeL.Infra.Repository.IRepositories;

namespace ThreeL.ContextAPI.Application.Impl.Services
{
    public class EmojiService : IEmojiService, IAppService
    {
        private readonly IAdoQuerierRepository<ContextAPIDbContext> _adoQuerierRepository;
        public EmojiService(IAdoQuerierRepository<ContextAPIDbContext> adoQuerierRepository)
        {
            _adoQuerierRepository = adoQuerierRepository;
        }

        public async Task<EmojiResponseDto> GetEmojiGroupsAsync(string endpoint, string applicationFolder)
        {
            EmojiResponseDto emojiResponse = new EmojiResponseDto();
            var groups = await _adoQuerierRepository.QueryAsync<EmojiGroup>("SELECT * FROM EMOJI WHERE ISDELETED = 0");
            foreach (var group in groups)
            {
                var folder = Path.Combine(applicationFolder, group.FolderLocation);
                if (Directory.Exists(folder))
                {
                    EmojiGroupDto emojiGroupDto = new EmojiGroupDto();
                    emojiGroupDto.GroupName = group.GroupName;
                    emojiGroupDto.GroupIcon = Path.Combine(endpoint + "/", group.RequestPath, group.GroupIcon);
                    DirectoryInfo directoryInfo = new DirectoryInfo(folder);
                    var files = directoryInfo.GetFiles();
                    emojiGroupDto.Emojis = files.Select(x => Path.Combine(endpoint + "/", group.RequestPath, x.Name));

                    emojiResponse.EmojiGroups.Add(emojiGroupDto);
                }
            }

            return emojiResponse;
        }
    }
}
