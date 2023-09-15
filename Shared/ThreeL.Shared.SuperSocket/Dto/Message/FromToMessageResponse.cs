using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    /// <summary>
    /// 所有收发消息的基类
    /// </summary>
    [ProtoContract]
    [ProtoInclude(7100, typeof(TextMessageResponse))]
    [ProtoInclude(7200, typeof(ImageMessageResponse))]
    [ProtoInclude(7300, typeof(FileMessageResponse))]
    [ProtoInclude(7400, typeof(WithdrawMessageResponse))]
    [ProtoInclude(7500, typeof(VoiceChatStatusResponse))]
    [ProtoInclude(7600, typeof(VoiceChatMessageResponse))]
    public class FromToMessageResponse : MessageResponse
    {
        [ProtoMember(5)]
        public long From { get; set; }
        [ProtoMember(6)]
        public string FromName { get; set; }
        [ProtoMember(7)]
        public long To { get; set; }
        [ProtoMember(8)]
        public bool IsGroup { get; set; }
    }
}
