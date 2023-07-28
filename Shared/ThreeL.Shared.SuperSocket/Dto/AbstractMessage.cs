using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto
{
    [ProtoContract]

    public class AbstractMessage : IMessage
    {
        [ProtoMember(1)]
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        [ProtoMember(2)]
        public string ReplyMessageId { get; set; }
        [ProtoMember(3)]
        public DateTime SendTime { get; set; }
        [ProtoMember(4)]
        public DateTime? ReceiveTime { get; set; }
    }
}
