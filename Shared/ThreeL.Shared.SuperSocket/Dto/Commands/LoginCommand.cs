using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Commands;

[ProtoContract]
public class LoginCommand : AbstractMessage
{
    [ProtoMember(4)]
    public long UserId { get; set; }
    [ProtoMember(5)]
    public string AccessToken { get; set; }
}
