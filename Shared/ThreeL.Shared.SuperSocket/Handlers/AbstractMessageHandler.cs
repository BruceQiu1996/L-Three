using SuperSocket;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Shared.SuperSocket.Handlers
{
    public abstract class AbstractMessageHandler : IMessageHandler
    {
        public bool Enable => true;

        public MessageType MessageType { get; private set; }

        public AbstractMessageHandler(MessageType messageType)
        {
            MessageType = messageType;
        }

        protected virtual async Task SendMessageBothAsync<TIPacket>(IEnumerable<IAppSession> fromSessions, IEnumerable<IAppSession> toSessions,
            long from, long to, IPacket resp) where TIPacket : class, IPacket
        {
            if (fromSessions != null)
            {
                foreach (var item in fromSessions)
                {
                    await item!.SendAsync(resp.Serialize());
                }
            }

            if (from != to)
            {
                if (toSessions != null)
                {
                    foreach (var item in toSessions)
                    {
                        await item!.SendAsync(resp.Serialize());
                    }
                }
            }
        }

        /// <summary>
        /// 执行逻辑
        /// </summary>
        /// <param name="appSession">client:null，server:client session</param>
        /// <param name="message">数据包</param>
        /// <returns></returns>
        public abstract Task ExcuteAsync(IAppSession appSession, IPacket message);
        /// <summary>
        /// 执行异常逻辑
        /// </summary>
        /// <param name="appSession">client:null，server:client session</param>
        /// <param name="message">数据包</param>
        /// <param name="ex">异常</param>
        /// <returns></returns>
        public virtual Task ExceptionAsync(IAppSession appSession, IPacket message, Exception ex) 
        {
            return Task.CompletedTask;
        }
    }
}
