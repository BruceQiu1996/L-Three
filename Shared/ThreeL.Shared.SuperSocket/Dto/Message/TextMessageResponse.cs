using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class TextMessageResponse : MessageResponse
    {
        [ProtoMember(6)]
        public string Text { get; set; }
        [ProtoMember(7)]
        public long From { get; set; }
        [ProtoMember(8)]
        public long To { get; set; }
    }
}
