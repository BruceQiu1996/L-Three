using ProtoBuf;
using ThreeL.Infra.Core.Metadata;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class VoiceChatStatusResponse : FromToMessageResponse
    {
        [ProtoMember(9)]
        public string ChatKey { get; set; }
        [ProtoMember(10)]
        public VoiceChatStatus Event { get; set; }
        [ProtoMember(11)]
        public DateTime SendTime { get; set; }
        [ProtoMember(12)]
        public int? ChatSeconds { get; set; }
    }
}
