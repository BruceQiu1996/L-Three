using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ThreeL.Client.Shared.Database;
using ThreeL.Client.Shared.Entities;
using ThreeL.Client.Shared.Services;
using ThreeL.Client.Shared.Utils;
using ThreeL.Client.Win.BackgroundService;
using ThreeL.Client.Win.Helpers;
using ThreeL.Client.Win.ViewModels;
using ThreeL.Client.Win.ViewModels.Messages;
using ThreeL.Infra.Core.Metadata;
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

        public override async Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var packet = message as Packet<FileMessageResponse>;
            HandleFromToMessageResponseFromServer(packet.Body);
            if (!packet.Body.Result)
                return;
            RelationViewModel friend = null;
            if (App.UserProfile.UserId == packet.Body.From)
            {
                friend = App.ServiceProvider.GetRequiredService<ChatViewModel>()
                    .RelationViewModels.FirstOrDefault(x => x.Id == packet.Body.To);
            }
            else
            {
                friend = App.ServiceProvider.GetRequiredService<ChatViewModel>()
                   .RelationViewModels.FirstOrDefault(x => x.Id == packet.Body.From);
            }

            if (friend != null)
            {
                //获取这个messageID对应的文件地址是否存在
                var path = _messageFileLocationMapper.Pop(packet.Body.MessageId);
                await _saveChatRecordService.WriteRecordAsync(new ChatRecord
                {
                    From = packet.Body.From,
                    To = packet.Body.To,
                    MessageId = packet.Body.MessageId,
                    Message = packet.Body.FileName,
                    MessageRecordType = MessageRecordType.File,
                    ResourceLocalLocation = path,
                    SendTime = packet.Body.SendTime,
                    FileId = packet.Body.FileId == 0 ? null : packet.Body.FileId,
                    ResourceSize = packet.Body.Size
                });

                if (App.UserProfile.UserId != packet.Body.From)
                {
                    var file = new FileMessageViewModel(packet.Body.FileName)
                    {
                        FileSize = packet.Body.Size,
                        SendTime = packet.Body.SendTime,
                        MessageId = packet.Body.MessageId,
                        From = packet.Body.From,
                        To = packet.Body.To,
                    };
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        friend.AddMessage(file);
                    });
                }
            }
        }
    }
}
