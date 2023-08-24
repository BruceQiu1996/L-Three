namespace ThreeL.Client.Shared.Dtos.ContextAPI
{
    public class FriendDto
    {
        public long ActiverId { get; set; }
        public string ActiverName { get; set; }
        public long PassiverId { get; set; }
        public string PassiverName { get; set; }
        public string ActiverRemark { get; set; }
        public string PassiverRemark { get; set; }
        public long? Avatar { get; set; }
    }
}
