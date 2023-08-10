using Microsoft.Extensions.DependencyInjection;
using SuperSocket;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ThreeL.Client.Shared.Database;
using ThreeL.Client.Shared.Entities;
using ThreeL.Client.Shared.Entities.Metadata;
using ThreeL.Client.Win.BackgroundService;
using ThreeL.Client.Win.Helpers;
using ThreeL.Client.Win.ViewModels;
using ThreeL.Client.Win.ViewModels.Messages;
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
        private readonly ClientSqliteContext _clientSqliteContext;
        private readonly SaveChatRecordService _saveChatRecordService;
        public ImageMessageResponseHandler(GrowlHelper growlHelper,
                                           ClientSqliteContext clientSqliteContext,
                                           SaveChatRecordService saveChatRecordService,
                                           FileHelper fileHelper) : base(MessageType.ImageResp)
        {
            _growlHelper = growlHelper;
            _fileHelper = fileHelper;
            _clientSqliteContext = clientSqliteContext;
            _saveChatRecordService = saveChatRecordService;
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
                var image = new ImageMessageViewModel()
                {
                    FromSelf = App.UserProfile.UserId == packet.Body.From,
                    ImageType = (ImageType)packet.Body.ImageType,
                    Url = packet.Body.Url,
                    SendTime = packet.Body.SendTime,
                    MessageId = packet.Body.MessageId,
                    From = packet.Body.From,
                    To = packet.Body.To,
                };

                string imageLocation = string.Empty;

                if (image.ImageType == ImageType.Local)
                {
                    image.Source = _fileHelper.ByteArrayToBitmapImage(packet.Body.ImageBytes);
                    imageLocation = await _fileHelper.AutoSaveImageAsync(packet.Body.ImageBytes, packet.Body.FileName);
                    if (string.IsNullOrEmpty(imageLocation))
                        return;
                }

                await _saveChatRecordService.WriteLogAsync(new ChatRecord
                {
                    From = packet.Body.From,
                    To = packet.Body.To,
                    MessageId = packet.Body.MessageId,
                    Message = image.ImageType == ImageType.Network ? "表情包" : packet.Body.FileName,
                    ResourceLocalLocation = image.ImageType == ImageType.Network ? image.Url : imageLocation,
                    MessageRecordType = MessageRecordType.Image,
                    ImageType = image.ImageType,
                    SendTime = packet.Body.SendTime
                });

                Application.Current.Dispatcher.Invoke(() =>
                {
                    friend.AddMessage(image);
                });
            }
        }
    }
}
