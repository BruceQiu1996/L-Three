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
    public class FromToMessageResponse : MessageResponse
    {
        [ProtoMember(6)]
        public long From { get; set; }
        [ProtoMember(7)]
        public long To { get; set; }
    }
}
