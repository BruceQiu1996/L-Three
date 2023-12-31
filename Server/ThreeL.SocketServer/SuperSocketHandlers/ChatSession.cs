﻿using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Channel;
using SuperSocket.Server;
using ThreeL.Shared.SuperSocket.Handlers;

namespace ThreeL.SocketServer.SuperSocketHandlers;
public class ChatSession : AppSession
{
    public long UserId { get; set; }
    public string UserName { get; set; }
    public string Platform { get; set; }
    public string AccessToken { get; set; }
    public string VoiceChatKey { get; set; }
    private ServerAppSessionManager<ChatSession> _sessionManager => Program.ServiceProvider.GetRequiredService<ServerAppSessionManager<ChatSession>>();

    protected override ValueTask OnSessionClosedAsync(CloseEventArgs e)
    {
        _sessionManager.TryRemoveBySessionId(UserId, SessionID);
        return base.OnSessionClosedAsync(e);
    }
}
