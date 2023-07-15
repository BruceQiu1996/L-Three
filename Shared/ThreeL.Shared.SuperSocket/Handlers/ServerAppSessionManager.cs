using SuperSocket;
using SuperSocket.Channel;
using System.Collections.Concurrent;

namespace ThreeL.Shared.SuperSocket.Handlers;
public class ServerAppSessionManager<TSession> : IDisposable where TSession : IAppSession
{
    /// <summary>
    /// 存储的Session
    /// </summary>
    public ConcurrentDictionary<string, TSession> Sessions { get; private set; } = new();

    /// <summary>
    /// Session的数量
    /// </summary>
    public int Count => Sessions.Count;

    /// <summary>
    /// </summary>
    public ServerAppSessionManager()
    {

    }

    public ConcurrentDictionary<string, TSession> GetAllSessions()
    {
        return Sessions;
    }

    /// <summary>
    /// 获取一个Session
    /// </summary>
    /// <param name="key"> </param>
    /// <returns> </returns>
    public TSession? TryGet(string key)
    {
        Sessions.TryGetValue(key, out var session);

        return session;
    }

    public bool TryAddOrUpdate(string key, TSession session)
    {
        if (Sessions.TryGetValue(key, out var oldSession))
        {
            return Sessions.TryUpdate(key, session, oldSession);
        }
        else
        {
            return Sessions.TryAdd(key, session);
        }
    }

    /// <summary>
    /// 移除一个Session
    /// </summary>
    /// <param name="key"> </param>
    /// <returns> </returns>
    public bool TryRemove(string key)
    {
        return Sessions.TryRemove(key, out var session);
    }

    /// <summary>
    /// 通过Session移除Session
    /// </summary>
    /// <param name="sessionId"> </param>
    /// <returns> </returns>
    public void TryRemoveBySessionId(string sessionId)
    {
        foreach (var session in Sessions)
        {
            if (session.Value.SessionID == sessionId)
            {
                Sessions.TryRemove(session);
                return;
            }
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="session"> </param>
    /// <param name="message"> </param>
    /// <returns> </returns>
    public virtual async Task SendAsync(IAppSession session, ReadOnlyMemory<byte> message)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        if (session.State != SessionState.Connected)
        {
            throw new InvalidOperationException("can't send message because session had closed.");
        }

        await session.SendAsync(message);
    }

    public async void Dispose()
    {
        foreach (var session in Sessions)
        {
            await session.Value!.CloseAsync(CloseReason.RemoteClosing);
        }
    }
}

