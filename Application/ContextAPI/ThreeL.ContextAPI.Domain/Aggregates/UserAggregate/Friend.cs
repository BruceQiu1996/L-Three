using ThreeL.Shared.Domain.Entities;

namespace ThreeL.ContextAPI.Domain.Aggregates.UserAggregate
{
    public class Friend : DomainEntity<long>
    {
        public long Activer { get; set; }
        public long Passiver { get; set; }
        public string ActiverRemark { get; set; }
        public string PassiverRemark { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }

        public string GetFriendRemarkName(long userId)
        {
            return userId == Activer ? ActiverRemark : PassiverRemark;
        }
    }
}
