using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Commands
{
    [ProtoContract]
    public class LoginCommandResponse : AbstractMessage
    {
        [ProtoMember(4)]
        public string SsToken { get; set; }
        [ProtoMember(5)]
        public bool Result { get; set; }
    }
}
