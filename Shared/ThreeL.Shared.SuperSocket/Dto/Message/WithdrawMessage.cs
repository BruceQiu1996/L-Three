using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class WithdrawMessage : FromToMessage
    {
        [ProtoMember(4)]
        public string WithdrawMessageId { get; set; }
    }
}
