using ThreeL.ContextAPI.Domain.Aggregates.UserAggregate.Metadata;
using ThreeL.Shared.Domain.Entities;

namespace ThreeL.ContextAPI.Domain.Aggregates.UserAggregate
{
    public class FriendApply : DomainEntity<long>
    {
        public long Activer { get; set; }
        public long Passiver { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ProcessTime { get; set; }
        public FriendApplyStatus Status { get; set; }
    }
}
