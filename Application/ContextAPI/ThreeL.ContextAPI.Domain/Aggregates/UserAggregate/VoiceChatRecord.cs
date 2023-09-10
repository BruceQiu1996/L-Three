using ThreeL.Infra.Core.Metadata;
using ThreeL.Shared.Domain.Entities;

namespace ThreeL.ContextAPI.Domain.Aggregates.UserAggregate
{
    public class VoiceChatRecord : DomainEntity<long>
    {
        public string ChatKey { get; set; } //特定生成的通话id key。
        public DateTime SendTime { get; set; }
        public long From { get; set; }
        public long To { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string FromPlatform { get; set; }
        public string ToPlatform { get; set; }
        public VioceChatRecordStatus Status { get; set; }
    }
}
