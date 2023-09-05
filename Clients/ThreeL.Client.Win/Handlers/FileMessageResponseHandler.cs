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
    public class FileMessageResponseHandler : ClientMessageHandler
    {
        private readonly GrowlHelper _growlHelper;
        private readonly FileHelper _fileHelper;
        private readonly ClientSqliteContext _clientSqliteContext;
        private readonly SaveChatRecordService _saveChatRecordService;
        private readonly ContextAPIService _contextAPIService;
        private readonly MessageFileLocationMapper _messageFileLocationMapper;
        public FileMessageResponseHandler(GrowlHelper growlHelper,
                                          ClientSqliteContext clientSqliteContext,
                                          SaveChatRecordService saveChatRecordService,
                                          ContextAPIService contextAPIService,
                                          MessageFileLocationMapper messageFileLocationMapper,
                                          FileHelper fileHelper) : base(MessageType.FileResp)
        {
            _growlHelper = growlHelper;
            _fileHelper = fileHelper;
            _clientSqliteContext = clientSqliteContext;
            _saveChatRecordService = saveChatRecordService;
            _messageFileLocationMapper = messageFileLocationMapper;
            _contextAPIService = contextAPIService;
        }

        public override Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var packet = message as Packet<FileMessageResponse>;
            WeakReferenceMessenger.Default.Send<FromToMessageResponse, string>(packet.Body, "message-receive");

            return Task.CompletedTask;
        }
    }
}
