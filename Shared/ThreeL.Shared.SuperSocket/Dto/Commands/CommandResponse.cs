using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Commands
{
    [ProtoContract]
    [ProtoInclude(100, typeof(LoginCommandResponse))]
    [ProtoInclude(200, typeof(RequestForUserEndpointCommandResponse))]
    public class CommandResponse : AbstractMessage
    {
        [ProtoMember(4)]
        public bool Result { get; set; }
        [ProtoMember(5)]
        public string Message { get; set; }
    }
}
