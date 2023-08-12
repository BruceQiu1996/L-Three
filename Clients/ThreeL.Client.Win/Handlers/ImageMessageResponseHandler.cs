using Microsoft.Extensions.DependencyInjection;
using SuperSocket;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ThreeL.Client.Shared.Database;
using ThreeL.Client.Shared.Entities;
using ThreeL.Client.Shared.Services;
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
    public class ImageMessageResponseHandler : AbstractMessageHandler
    {
        private readonly GrowlHelper _growlHelper;
        private readonly FileHelper _fileHelper;
        private readonly ClientSqliteContext _clientSqliteContext;
        private readonly SaveChatRecordService _saveChatRecordService;
        private readonly ContextAPIService _contextAPIService;
        public ImageMessageResponseHandler(GrowlHelper growlHelper,
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
                    ImageType = packet.Body.ImageType,
                    Url = packet.Body.RemoteUrl,
                    SendTime = packet.Body.SendTime,
                    MessageId = packet.Body.MessageId,
                    From = packet.Body.From,
                    To = packet.Body.To,
                };

                string imageLocation = string.Empty;

                if (image.ImageType == ImageType.Local)
                {
                    var result = await _fileHelper.AutoSaveImageAsync(packet.Body.FileBase64, packet.Body.FileName);
                    if(result==default)
                    {
                        _growlHelper.Warning("接收图片出现异常");
                        return;
                    }
                    image.Source = _fileHelper.ByteArrayToBitmapImage(result.raw);
                    imageLocation = result.location;
                }

                await _saveChatRecordService.WriteRecordAsync(new ChatRecord
                {
                    From = packet.Body.From,
                    To = packet.Body.To,
                    MessageId = packet.Body.MessageId,
                    Message = image.ImageType == ImageType.Network ? "表情包" : packet.Body.FileName,
                    ResourceLocalLocation = image.ImageType == ImageType.Network ? image.Url : imageLocation,
                    MessageRecordType = MessageRecordType.Image,
                    ImageType = image.ImageType,
                    SendTime = packet.Body.SendTime,
                    FileId = packet.Body.FileId,
                });

                Application.Current.Dispatcher.Invoke(() =>
                {
                    friend.AddMessage(image);
                });
            }
        }
    }
}
