using ThreeL.ContextAPI.Domain.Aggregates.UserAggregate.Metadata;

namespace ThreeL.ContextAPI.Application.Contract.Dtos.Relation
{
    public class FriendApplyResponseDto
    {
        public long Id { get; set; }
        public long ActiverId { get; set; }
        public string ActiverName { get; set; }
        public long PassiverId { get; set; }
        public string PassiverName { get; set; }
        public bool FromSelf { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ProcessTime { get; set; }
        public FriendApplyStatus Status { get; set; }
    }
}
