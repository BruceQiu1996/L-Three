namespace ThreeL.Shared.SuperSocket.Dto.Commands;
public class LoginCommand : AbstractMessage
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Token { get; set; }
    public string Source { get; set; }
}
