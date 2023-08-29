namespace ThreeL.Client.Shared.Dtos.ContextAPI
{
    public class UserRoughlyDto
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public long? Avatar { get; set; }
        public string Sign { get; set; }
        public bool IsFriend { get; set; } //已经建立好友关系
        public string Role { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
