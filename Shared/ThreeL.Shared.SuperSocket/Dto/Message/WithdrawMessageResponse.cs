using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class WithdrawMessageResponse : FromToMessageResponse
    {
        [ProtoMember(8)]
        public string WithdrawMessageId { get; set; }
    }
}
