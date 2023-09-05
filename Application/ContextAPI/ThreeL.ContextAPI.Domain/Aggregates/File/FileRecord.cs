using ThreeL.Infra.Repository.Entities;
using ThreeL.Shared.Domain.Entities;

namespace ThreeL.ContextAPI.Domain.Aggregates.File
{
    public class FileRecord : AggregateRoot<long>, IBasicAuditInfo<long>
    {
        public string FileName { get; set; }
        public string Code { get; set; }
        public string Location { get; set; }
        public long Size { get; set; }
        public long Receiver { get; set; }
        public long CreateBy { get; set; }
        public DateTime CreateTime { get; set; }
        public bool IsGroup { get; set; }
    }
}
