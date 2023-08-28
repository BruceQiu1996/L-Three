namespace ThreeL.ContextAPI.Application.Contract.Dtos.Relation
{
    public class FriendDto
    {
        public long ActiverId { get; set; }
        public string ActiverName { get; set; }
        public long PassiverId { get; set; }
        public string PassiverName { get; set; }
        public string ActiverRemark { get; set; }
        public string PassiverRemark { get; set; }
        public long? ActiverAvatar { get; set; }
        public long? PassiverAvatar { get; set; }
        public DateTime CreateTime { get; set; }

        public string GetFriendName(long id)
        {
            return ActiverId == id ? PassiverName : ActiverName;
        }

        public string GetFriendRemark(long id)
        {
            return ActiverId == id ? PassiverRemark : ActiverRemark;
        }

        public long? GetFriendAvatar(long id)
        {
            return ActiverId == id ? PassiverAvatar : ActiverAvatar;
        }

        public long GetFriendId(long id)
        {
            return ActiverId == id ? PassiverId : ActiverId;
        }
    }
}
