using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class TextMessage : FromToMessage
    {
        [ProtoMember(4)]
        public string Text { get; set; }
    }
}
