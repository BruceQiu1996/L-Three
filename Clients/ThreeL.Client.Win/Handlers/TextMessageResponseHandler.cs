using CommunityToolkit.Mvvm.Messaging;
using SuperSocket;
using System.Threading.Tasks;
using ThreeL.Client.Win.BackgroundService;
using ThreeL.Client.Win.Helpers;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.Handlers
{
    public class TextMessageResponseHandler : ClientMessageHandler
    {
        private readonly GrowlHelper _growlHelper;
        private readonly SaveChatRecordService _saveChatRecordService;
        public TextMessageResponseHandler(GrowlHelper growlHelper, SaveChatRecordService saveChatRecordService) : base(MessageType.TextResp)
        {
            _growlHelper = growlHelper;
            _saveChatRecordService = saveChatRecordService;
        }

        public async override Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var packet = message as Packet<TextMessageResponse>;
            WeakReferenceMessenger.Default.Send<FromToMessageResponse, string>(packet.Body, "message-receive");
        }
    }
}
