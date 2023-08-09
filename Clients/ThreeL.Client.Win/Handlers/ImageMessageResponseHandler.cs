using Microsoft.Extensions.DependencyInjection;
using SuperSocket;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ThreeL.Client.Shared.Entities.Metadata;
using ThreeL.Client.Win.Helpers;
using ThreeL.Client.Win.ViewModels;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.Handlers
{
    public class ImageMessageResponseHandler : AbstractMessageHandler
    {
        private readonly GrowlHelper _growlHelper;
        private readonly FileHelper _fileHelper;
        public ImageMessageResponseHandler(GrowlHelper growlHelper, FileHelper fileHelper) : base(MessageType.ImageResp)
        {
            _growlHelper = growlHelper;
            _fileHelper = fileHelper;
        }

        public override async Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var packet = message as Packet<ImageMessageResponse>;
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
                var image = new ViewModels.Messages.ImageMessage()
                {
                    FromSelf = App.UserProfile.UserId == packet.Body.From,
                    ImageType = (ImageType)packet.Body.ImageType,
                    Url = packet.Body.Url,
                    SendTime = packet.Body.SendTime,
                    MessageId = packet.Body.MessageId,
                    From = packet.Body.From,
                    To = packet.Body.To,
                };

                if (image.ImageType == ImageType.Local)
                {
                    //TODO byte[]转bitmapImage
                    await _fileHelper.AutoSaveImageAsync(packet.Body.ImageBytes,packet.Body.FileName);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    friend.Messages.Add(image);
                });
            }
        }
    }
}
