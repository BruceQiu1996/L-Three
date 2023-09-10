using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    /// <summary>
    /// 发起语音通话的消息
    /// </summary>
    [ProtoContract]
    public class ApplyforVoiceChatMessage : FromToMessage
    {
        [ProtoMember(4)]
        public string Platform { get; set; }
    }
}
