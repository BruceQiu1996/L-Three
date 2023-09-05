using CommunityToolkit.Mvvm.Messaging;
using Dapper;
using SuperSocket;
using System.IO;
using System.Threading.Tasks;
using ThreeL.Client.Shared.Database;
using ThreeL.Client.Shared.Entities;
using ThreeL.Client.Shared.Services;
using ThreeL.Client.Shared.Utils;
using ThreeL.Client.Win.BackgroundService;
using ThreeL.Client.Win.Helpers;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.Handlers
{
    public class WithdrawMessageResponseHandler : ClientMessageHandler
    {
        private readonly FileHelper _fileHelper;
        private readonly ClientSqliteContext _clientSqliteContext;
        private readonly SaveChatRecordService _saveChatRecordService;
        private readonly ContextAPIService _contextAPIService;
        private readonly MessageFileLocationMapper _messageFileLocationMapper;
        public WithdrawMessageResponseHandler(ClientSqliteContext clientSqliteContext,
                                              SaveChatRecordService saveChatRecordService,
                                              ContextAPIService contextAPIService,
                                              MessageFileLocationMapper messageFileLocationMapper,
                                              FileHelper fileHelper) : base(MessageType.WithdrawResp)
        {
            _fileHelper = fileHelper;
            _clientSqliteContext = clientSqliteContext;
            _saveChatRecordService = saveChatRecordService;
            _messageFileLocationMapper = messageFileLocationMapper;
            _contextAPIService = contextAPIService;
        }

        public async override Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var packet = message as Packet<WithdrawMessageResponse>;
            WeakReferenceMessenger.Default.Send<WithdrawMessageResponse, string>(packet.Body, "message-withdraw-result");
            var record = await SqlMapper.QueryFirstOrDefaultAsync<ChatRecord>(_clientSqliteContext.dbConnection, "SELECT * FROM ChatRecord WHERE MessageId = @MessageId", new
            {
                packet.Body.WithdrawMessageId,
                packet.Body.From
            });

            if (record != null)
            {
                await SqlMapper.ExecuteAsync(_clientSqliteContext.dbConnection, "DELETE ChatRecord where MessageId = @MessageId", new
                {
                    record.MessageId
                });

                if (File.Exists(record.ResourceLocalLocation) && App.UserProfile.UserId != packet.Body.From)
                {
                    File.Delete(record.ResourceLocalLocation);
                }
            }
        }
    }
}
