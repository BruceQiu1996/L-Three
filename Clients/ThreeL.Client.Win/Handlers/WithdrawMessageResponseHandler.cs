﻿using CommunityToolkit.Mvvm.Messaging;
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
using ThreeL.Infra.Core.Metadata;
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
            HandleFromToMessageResponseFromServer(packet.Body);
            if (!packet.Body.Result)
                return;

            WeakReferenceMessenger.Default.Send<WithdrawMessageResponse, string>(packet.Body, "message-withdraw-result");
            var record = await SqlMapper.QueryFirstOrDefaultAsync<ChatRecord>(_clientSqliteContext.dbConnection, "SELECT Top 1 * FROM ChatRecord WHERE MessageId = @MessageId AND [From] = @From", new
            {
                packet.Body.WithdrawMessageId,
                packet.Body.From
            });

            if (record != null)
            {
                await SqlMapper.ExecuteAsync(_clientSqliteContext.dbConnection, "UPDATE ChatRecord SET ResourceLocalLocation = null,FileId = null,MessageRecordType = @MessageType where MessageId = @MessageId", new
                {
                    MessageType = MessageRecordType.Withdraw,
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