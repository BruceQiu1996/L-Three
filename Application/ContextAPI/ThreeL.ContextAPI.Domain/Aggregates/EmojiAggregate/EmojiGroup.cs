using ThreeL.Infra.Repository.Entities;
using ThreeL.Shared.Domain.Entities;

namespace ThreeL.ContextAPI.Domain.Aggregates.EmojiAggregate
{
    public class EmojiGroup : AggregateRoot<long>, ISoftDelete
    {
        public string GroupName { get; set; }
        public string GroupIcon { get; set; }
        public string RequestPath { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreateTime { get; set; }
        public string FolderLocation { get; set; }
    }
}
