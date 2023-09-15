using ProtoBuf;
using ThreeL.Infra.Core.Metadata;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class VoiceChatMessageResponse : FromToMessageResponse
    {
        [ProtoMember(9)]
        public VoiceChatStatus Status { get; set; }
        [ProtoMember(10)]
        public int? ChatSeconds { get; set; }
    }
}
