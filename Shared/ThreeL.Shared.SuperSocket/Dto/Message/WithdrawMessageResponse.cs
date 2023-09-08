using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class WithdrawMessageResponse : FromToMessageResponse
    {
        [ProtoMember(9)]
        public string WithdrawMessageId { get; set; }
    }
}
