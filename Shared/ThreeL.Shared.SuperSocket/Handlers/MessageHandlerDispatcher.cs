using Microsoft.Extensions.Logging;
using SuperSocket;
using ThreeL.Shared.SuperSocket.Dto;

namespace ThreeL.Shared.SuperSocket.Handlers
{
    public class MessageHandlerDispatcher
    {
        private readonly IEnumerable<IMessageHandler> _messageHandlers;
        private readonly ILogger<MessageHandlerDispatcher> _logger;

        public MessageHandlerDispatcher(IEnumerable<IMessageHandler> messageHandlers, ILogger<MessageHandlerDispatcher> logger)
        {
            _messageHandlers = messageHandlers;
            _logger = logger;
        }

        public async virtual Task DispatcherMessageHandlerAsync(IAppSession appSession, IPacket message)
        {
            var handler = _messageHandlers.FirstOrDefault(x => x.Name == message.MessageType.ToString());
            if (handler == null)
            {
                _logger.LogError($"can't find {nameof(message.MessageType)} dispatcher.");

                return;
            }

            await handler.ExcuteAsync(appSession, message);
        }
    }
}
