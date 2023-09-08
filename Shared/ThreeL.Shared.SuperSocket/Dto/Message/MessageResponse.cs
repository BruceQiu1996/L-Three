using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    [ProtoInclude(1100, typeof(FromToMessageResponse))]
    public abstract class MessageResponse : AbstractMessage
    {
        [ProtoMember(2)]
        public DateTime SendTime { get; set; } = DateTime.Now;
        [ProtoMember(3)]
        public bool Result { get; set; }
        [ProtoMember(4)]
        public string Message { get; set; }
    }
}
