using CommunityToolkit.Mvvm.Messaging;
using SuperSocket;
using System.Threading.Tasks;
using ThreeL.Client.Shared.Database;
using ThreeL.Client.Shared.Services;
using ThreeL.Client.Shared.Utils;
using ThreeL.Client.Win.BackgroundService;
using ThreeL.Client.Win.Helpers;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.Handlers
{
    public class ImageMessageResponseHandler : ClientMessageHandler
    {
        private readonly GrowlHelper _growlHelper;
        private readonly FileHelper _fileHelper;
        private readonly ClientSqliteContext _clientSqliteContext;
        private readonly SaveChatRecordService _saveChatRecordService;
        private readonly MessageFileLocationMapper _messageFileLocationMapper;
        private readonly ContextAPIService _contextAPIService;
        public ImageMessageResponseHandler(GrowlHelper growlHelper,
                                           MessageFileLocationMapper messageFileLocationMapper,
                                           ClientSqliteContext clientSqliteContext,
                                           SaveChatRecordService saveChatRecordService,
                                           ContextAPIService contextAPIService,
                                           FileHelper fileHelper) : base(MessageType.ImageResp)
        {
            _growlHelper = growlHelper;
            _fileHelper = fileHelper;
            _clientSqliteContext = clientSqliteContext;
            _saveChatRecordService = saveChatRecordService;
            _contextAPIService = contextAPIService;
            _messageFileLocationMapper = messageFileLocationMapper;
        }

        public override Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var packet = message as Packet<ImageMessageResponse>;
            WeakReferenceMessenger.Default.Send<FromToMessageResponse, string>(packet.Body, "message-receive");

            return Task.CompletedTask;
        }
    }
}
