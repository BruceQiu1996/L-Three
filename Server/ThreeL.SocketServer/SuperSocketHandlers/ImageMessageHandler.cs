using SuperSocket;
using ThreeL.Infra.Redis;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.Shared.SuperSocket.Metadata;
using ThreeL.SocketServer.Application.Contract.Configurations;

namespace ThreeL.SocketServer.SuperSocketHandlers
{
    public class ImageMessageHandler : AbstractMessageHandler
    {
        private readonly ServerAppSessionManager<ChatSession> _sessionManager;
        private readonly IRedisProvider _redisProvider;

        public ImageMessageHandler(ServerAppSessionManager<ChatSession> sessionManager, IRedisProvider redisProvider) : base(MessageType.Image)
        {
            _redisProvider = redisProvider;
            _sessionManager = sessionManager;
        }

        public override async Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var packet = message as Packet<ImageMessage>;
            var resp = new Packet<ImageMessageResponse>()
            {
                Sequence = packet.Sequence,
                Checkbit = packet.Checkbit,
                MessageType = MessageType.ImageResp,
            };

            var body = new ImageMessageResponse();
            resp.Body = body;
            if (packet.Body.From != packet.Body.To)
            {
                var e1 = await _redisProvider.SetIsMemberAsync(Const.FRIEND_RELATIONS, $"{packet.Body.From}-{packet.Body.To}");
                var e2 = await _redisProvider.SetIsMemberAsync(Const.FRIEND_RELATIONS, $"{packet.Body.To}-{packet.Body.From}");
                if ((e1 || e2) == false)
                {
                    body.Result = false;
                    body.Message = "好友关系异常";
                    await appSession.SendAsync(resp.Serialize());
                    return;
                }
            }

            body.Result = true;
            body.Url = packet.Body.Url;
            body.ImageType = packet.Body.ImageType;
            body.ImageBytes = packet.Body.ImageBytes;
            body.From = packet.Body.From;
            body.To = packet.Body.To;
            body.FileName = packet.Body.FileName;
            body.SendTime = packet.Body.SendTime;
            //TODO保存聊天记录到数据库
            //如果是本地图片，还需要存储到目录下。
            //分发给发送者和接收者
            var fromSessions = _sessionManager.TryGet(resp.Body.From);
            var toSessions = _sessionManager.TryGet(resp.Body.To);
            if (fromSessions != null)
            {
                foreach (var item in fromSessions)
                {
                    await (item! as IAppSession).SendAsync(resp.Serialize());
                }
            }

            if (packet.Body.From != packet.Body.To)
            {
                if (toSessions != null)
                {
                    foreach (var item in toSessions)
                    {
                        await (item! as IAppSession).SendAsync(resp.Serialize());
                    }
                }
            }
        }
    }
}
