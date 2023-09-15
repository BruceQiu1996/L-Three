using ProtoBuf;
using ThreeL.Infra.Core.Metadata;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class FinishVoiceChatMessage : FromToMessage
    {
        [ProtoMember(4)]
        public string Chatkey { get; set; }
        [ProtoMember(5)]
        public VoiceChatStatus Action { get; set; }
    }
}
