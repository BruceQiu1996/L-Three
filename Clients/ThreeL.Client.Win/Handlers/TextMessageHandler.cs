using Microsoft.Extensions.DependencyInjection;
using SuperSocket;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ThreeL.Client.Win.ViewModels;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.Handlers
{
    public class TextMessageHandler : AbstractMessageHandler
    {
        public TextMessageHandler() : base(MessageType.Text)
        {
        }

        public override Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var packet = message as Packet<TextMessage>;
            var friend = App.ServiceProvider.GetRequiredService<ChatViewModel>().FriendViewModels.FirstOrDefault(x => x.Id == packet.Body.From);
            if (friend != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    friend.Messages.Add(new ViewModels.Messages.TextMessage()
                    {
                        FromSelf = true,
                        Text = packet.Body.Text,
                    });
                });
            }

            return Task.CompletedTask;
        }
    }
}
