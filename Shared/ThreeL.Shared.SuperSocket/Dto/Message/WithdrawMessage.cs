using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class WithdrawMessage : FromToMessage
    {
        [ProtoMember(5)]
        public string WithdrawMessageId { get; set; }
    }
}
