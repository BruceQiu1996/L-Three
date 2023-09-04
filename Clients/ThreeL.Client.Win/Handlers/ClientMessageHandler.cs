using CommunityToolkit.Mvvm.Messaging;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.Handlers
{
    public abstract class ClientMessageHandler : AbstractMessageHandler
    {
        protected ClientMessageHandler(MessageType messageType) : base(messageType)
        {
        }

        /// <summary>
        /// 用于客户端处理从服务器的from-to消息的响应
        /// </summary>
        protected virtual void HandleFromToMessageResponseFromServer(FromToMessageResponse response)
        {
            WeakReferenceMessenger.Default.Send(response, "message-send-finished");
            WeakReferenceMessenger.Default.Send(response, "message-send-result");
        }
    }
}
