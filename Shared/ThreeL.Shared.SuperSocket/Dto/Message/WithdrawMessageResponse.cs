using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class WithdrawMessageResponse : FromToMessageResponse
    {
        [ProtoMember(10)]
        public string WithdrawMessageId { get; set; }
    }
}
