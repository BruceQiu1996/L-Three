using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Commands
{
    [ProtoContract]
    public class RequestForUserEndpointCommandResponse : CommandResponse
    {
        [ProtoMember(6)]
        public string Addresses { get; set; } 
    }
}
