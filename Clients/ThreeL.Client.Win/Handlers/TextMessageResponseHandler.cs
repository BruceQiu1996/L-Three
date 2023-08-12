using Microsoft.Extensions.DependencyInjection;
using SuperSocket;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ThreeL.Client.Shared.Entities;
using ThreeL.Client.Win.BackgroundService;
using ThreeL.Client.Win.Helpers;
using ThreeL.Client.Win.ViewModels;
using ThreeL.Client.Win.ViewModels.Messages;
using ThreeL.Infra.Core.Metadata;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.Handlers
{
    public class TextMessageResponseHandler : AbstractMessageHandler
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
            if (packet != null && !packet.Body.Result) //消息发送失败
            {
                _growlHelper.Warning(packet.Body.Message);
                return;
            }
            FriendViewModel friend = null;
            if (App.UserProfile.UserId == packet.Body.From)
            {
                friend = App.ServiceProvider.GetRequiredService<ChatViewModel>()
                    .FriendViewModels.FirstOrDefault(x => x.Id == packet.Body.To);
            }
            else
            {
                friend = App.ServiceProvider.GetRequiredService<ChatViewModel>()
                   .FriendViewModels.FirstOrDefault(x => x.Id == packet.Body.From);
            }

            if (friend != null)
            {
                await _saveChatRecordService.WriteRecordAsync(new ChatRecord
                {
                    From = packet.Body.From,
                    To = packet.Body.To,
                    MessageId = packet.Body.MessageId,
                    Message = packet.Body.Text,
                    MessageRecordType = MessageRecordType.Text,
                    SendTime = packet.Body.SendTime
                });

                Application.Current.Dispatcher.Invoke(() =>
                {
                    friend.AddMessage(new TextMessageViewModel()
                    {
                        FromSelf = App.UserProfile.UserId == packet.Body.From,
                        Text = packet.Body.Text,
                        SendTime = packet.Body.SendTime,
                        MessageId = packet.Body.MessageId,
                        From = packet.Body.From,
                        To = packet.Body.To,
                    });
                });
            }
        }
    }
}
