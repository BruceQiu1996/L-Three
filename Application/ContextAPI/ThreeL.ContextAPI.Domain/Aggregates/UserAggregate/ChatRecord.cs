using ThreeL.Infra.Core.Metadata;
using ThreeL.Shared.Domain.Entities;

namespace ThreeL.ContextAPI.Domain.Aggregates.UserAggregate
{
    /// <summary>
    /// 服务端聊天记录，TODO分区/分表
    /// </summary>
    public class ChatRecord : DomainEntity<long>
    {
        public string MessageId { get; set; }
        public string Message { get; set; }
        public MessageRecordType MessageRecordType { get; set; }
        public ImageType ImageType { get; set; }
        public DateTime SendTime { get; set; }
        public long From { get; set; }
        public long To { get; set; }
        public long? FileId { get; set; } 
    }
}
