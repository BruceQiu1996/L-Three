namespace ThreeL.ContextAPI.Application.Contract.Dtos.Emoji
{
    public class EmojiResponseDto
    {
        public EmojiResponseDto()
        {
            EmojiGroups = new List<EmojiGroupDto>();
        }

        public List<EmojiGroupDto> EmojiGroups { get; set; }
    }

    public class EmojiGroupDto
    {
        public string GroupName { get; set; }
        public string GroupIcon { get; set; }
        public IEnumerable<string> Emojis { get; set; }
    }
}
