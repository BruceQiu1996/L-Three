using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class TextMessage : AbstractMessage
    {
        [ProtoMember(4)]
        public string Text { get; set; }
    }
}
