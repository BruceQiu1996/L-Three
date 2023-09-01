using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class TextMessageResponse : FromToMessageResponse
    {
        [ProtoMember(9)]
        public string Text { get; set; }
    }
}
