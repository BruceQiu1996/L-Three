using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class WithdrawMessage : FromToMessage
    {
        [ProtoMember(6)]
        public string WithdrawMessageId { get; set; }
    }
}
