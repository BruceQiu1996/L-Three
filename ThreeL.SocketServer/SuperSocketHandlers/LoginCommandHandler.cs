using SuperSocket;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Commands;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.SocketServer.SuperSocketHandlers;
public class LoginCommandHandler : AbstractMessageHandler
{
    private readonly ServerAppSessionManager<ChatSession> _sessionManager;

    public LoginCommandHandler(ServerAppSessionManager<ChatSession> sessionManager) : base(nameof(MessageType.Login))
    {
        _sessionManager = sessionManager;
    }

    public override Task ExcuteAsync(IAppSession appSession, IPacket message)
    {
        var session = appSession as ChatSession;
        var packet = message as Packet<LoginCommand>;
        session.UserId = packet.Body.UserId;
        session.UserName = packet.Body.UserName;
        _sessionManager.TryAddOrUpdate(session.UserId, session);

        return Task.CompletedTask;
    }
}
