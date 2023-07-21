using SuperSocket;
using SuperSocket.Channel;
using System.Collections.Concurrent;

namespace ThreeL.Shared.SuperSocket.Handlers;
public class ServerAppSessionManager<TSession> : IDisposable where TSession : IAppSession
{
    /// <summary>
    /// 存储的Session
    /// </summary>
    public ConcurrentDictionary<string, List<TSession>> Sessions { get; private set; } = new();

    /// <summary>
    /// Session的数量
    /// </summary>
    public int Count => Sessions.SelectMany(x => x.Value).Count();

    /// <summary>
    /// </summary>
    public ServerAppSessionManager()
    {

    }

    /// <summary>
    /// 获取某人的连接
    /// </summary>
    /// <param name="key"> </param>
    /// <returns> </returns>
    public IEnumerable<TSession?> TryGet(string key)
    {
        Sessions.TryGetValue(key, out var session);

        return session;
    }

    public bool TryAddOrUpdate(string key, TSession session)
    {
        Sessions.AddOrUpdate(key, new List<TSession> { session }, (k, v) =>
        {
            v.Add(session);
            return v;
        });

        return true;
    }

    /// <summary>
    /// 通过Session移除Session
    /// </summary>
    /// <param name="sessionId"> </param>
    /// <returns> </returns>
    public void TryRemoveBySessionId(string userId, string sessionId)
    {
        if (string.IsNullOrEmpty(userId))
            return;

        if (!Sessions.ContainsKey(userId))
            return;

        var sessions = Sessions[userId];
        sessions.RemoveAll(x => x.SessionID == sessionId);
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
        foreach (var item in Sessions)
        {
            foreach (var session in item.Value)
            {
                await session.CloseAsync(CloseReason.ServerShutdown);
            }
        }
    }
}

