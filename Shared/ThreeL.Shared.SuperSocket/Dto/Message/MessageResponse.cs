using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    [ProtoInclude(1100, typeof(TextMessageResponse))]
    [ProtoInclude(1300, typeof(ImageMessageResponse))]
    [ProtoInclude(1400, typeof(FileMessageResponse))]
    public abstract class MessageResponse : AbstractMessage
    {
        [ProtoMember(4)]
        public bool Result { get; set; }
        [ProtoMember(5)]
        public string Message { get; set; }
    }
}
