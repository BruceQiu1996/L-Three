using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    /// <summary>
    /// 所有收发消息的基类
    /// </summary>
    [ProtoContract]
    [ProtoInclude(6100, typeof(TextMessage))]
    [ProtoInclude(6200, typeof(ImageMessage))]
    [ProtoInclude(6300, typeof(FileMessage))]
    [ProtoInclude(6400, typeof(WithdrawMessage))]
    public abstract class FromToMessage : AbstractMessage
    {
        [ProtoMember(4)]
        public long To { get; set; }
    }
}
