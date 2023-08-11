using SuperSocket;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Commands;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.Shared.SuperSocket.Metadata;
using ThreeL.SocketServer.Application.Contract;
using ThreeL.SocketServer.Application.Contract.Services;

namespace ThreeL.SocketServer.SuperSocketHandlers;
public class LoginCommandHandler : AbstractMessageHandler
{
    private readonly ServerAppSessionManager<ChatSession> _sessionManager;
    private readonly IContextAPIGrpcService _contextAPIGrpc;

    public LoginCommandHandler(ServerAppSessionManager<ChatSession> sessionManager,
                               IContextAPIGrpcService contextAPIGrpc) : base(MessageType.Login)
    {
        _sessionManager = sessionManager;
        _contextAPIGrpc = contextAPIGrpc;
    }

    public async override Task ExcuteAsync(IAppSession appSession, IPacket message)
    {
        var session = appSession as ChatSession;
        var packet = message as Packet<LoginCommand>;
        _contextAPIGrpc.SetToken(packet.Body.AccessToken);
        var resp = await _contextAPIGrpc.SocketServerUserLoginAsync(new SocketServerUserLoginRequest());
        if (resp.Result)
        {
            session.UserId = packet.Body.UserId;
            session.SsToken = Guid.NewGuid().ToString();
            _sessionManager.TryAddOrUpdate(session.UserId, session);
        }

        await appSession.SendAsync(new Packet<LoginCommandResponse>()
        {
            MessageType = MessageType.LoginResponse,
            Sequence = packet.Sequence,
            Body = new LoginCommandResponse()
            {
                Result = resp.Result,
                SsToken = resp.Result ? session.SsToken : null,
            }
        }.Serialize());
    }
}
