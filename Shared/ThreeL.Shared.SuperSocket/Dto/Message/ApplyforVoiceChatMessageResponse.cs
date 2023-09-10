using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class ApplyforVoiceChatMessageResponse : FromToMessageResponse
    {
        [ProtoMember(9)]
        public string ChatKey { get; set; }
        [ProtoMember(10)]
        public long From { get; set; }
        [ProtoMember(11)]
        public long To { get; set; }
        [ProtoMember(12)]
        public string FromName { get; set; }
    }
}
