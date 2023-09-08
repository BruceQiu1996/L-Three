using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Commands
{
    [ProtoContract]

    public class RequestForUserEndpointCommand : AbstractMessage
    {
        [ProtoMember(2)]
        public long UserId { get; set; }
        [ProtoMember(3)]
        public string SsToken { get; set; }
    }
}
