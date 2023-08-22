using ThreeL.Infra.Repository.Entities;
using ThreeL.Shared.Domain.Entities;

namespace ThreeL.ContextAPI.Domain.Aggregates.UserAggregate
{
    public class UserAvatar : AggregateRoot<long>, IBasicAuditInfo<long>
    {
        public string FileName { get; set; }
        public string Code { get; set; }
        public string Location { get; set; }
        public long CreateBy { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
