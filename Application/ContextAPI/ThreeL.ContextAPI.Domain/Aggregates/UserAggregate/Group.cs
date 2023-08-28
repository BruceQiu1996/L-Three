using ThreeL.Infra.Repository.Entities;
using ThreeL.Shared.Domain.Entities;

namespace ThreeL.ContextAPI.Domain.Aggregates.UserAggregate
{
    public class Group : DomainEntity<int>, IBasicAuditInfo<long>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public long? Avatar { get; set; }
        public string Members { get; set; }
        public long CreateBy { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
