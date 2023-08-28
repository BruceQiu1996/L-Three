namespace ThreeL.Client.Shared.Dtos.ContextAPI
{
    public class GroupCreationResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public long? Avatar { get; set; }
        public long CreateBy { get; set; }
        public DateTime CreateTime { get; set; }
        public IEnumerable<UserRoughlyDto> Users { get; set; }
    }
}
