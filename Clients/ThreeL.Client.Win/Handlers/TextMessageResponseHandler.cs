using Microsoft.Extensions.DependencyInjection;
using SuperSocket;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ThreeL.Client.Win.Helpers;
using ThreeL.Client.Win.ViewModels;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.Handlers
{
    public class TextMessageResponseHandler : AbstractMessageHandler
    {
        private readonly GrowlHelper _growlHelper;
        public TextMessageResponseHandler(GrowlHelper growlHelper) : base(MessageType.TextResp) 
        {
            _growlHelper = growlHelper;
        }

        public override Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var packet = message as Packet<TextMessageResponse>;
            if (packet != null && !packet.Body.Result) //消息发送失败
            {
                _growlHelper.Warning(packet.Body.Message);
                return Task.CompletedTask;
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
                Application.Current.Dispatcher.Invoke(() =>
                {
                    friend.Messages.Add(new ViewModels.Messages.TextMessage()
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

            return Task.CompletedTask;
        }
    }
}
