using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class TextMessageResponse : FromToMessageResponse
    {
        [ProtoMember(10)]
        public string Text { get; set; }
    }
}
