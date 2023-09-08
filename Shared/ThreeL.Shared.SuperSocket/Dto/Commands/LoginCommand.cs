using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Commands;

[ProtoContract]
public class LoginCommand : AbstractMessage
{
    [ProtoMember(2)]
    public long UserId { get; set; }
    [ProtoMember(3)]
    public string AccessToken { get; set; }
    [ProtoMember(4)]
    public string Platform { get; set; }
}
