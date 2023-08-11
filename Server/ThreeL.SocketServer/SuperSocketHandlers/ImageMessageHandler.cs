using AutoMapper;
using SuperSocket;
using ThreeL.Infra.Redis;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.Shared.SuperSocket.Metadata;
using ThreeL.SocketServer.Application.Contract;
using ThreeL.SocketServer.Application.Contract.Services;

namespace ThreeL.SocketServer.SuperSocketHandlers
{
    public class ImageMessageHandler : AbstractMessageHandler
    {
        private readonly IContextAPIGrpcService _contextAPIGrpcService;
        private readonly IMessageHandlerService _messageHandlerService;
        private readonly ServerAppSessionManager<ChatSession> _sessionManager;
        private readonly IRedisProvider _redisProvider;
        private readonly IMapper _mapper;

        public ImageMessageHandler(ServerAppSessionManager<ChatSession> sessionManager,
                                   IRedisProvider redisProvider,
                                   IContextAPIGrpcService contextAPIGrpcService,
                                   IMapper mapper,
                                   IMessageHandlerService messageHandlerService) : base(MessageType.Image)
        {
            _mapper = mapper;
            _redisProvider = redisProvider;
            _sessionManager = sessionManager;
            _messageHandlerService = messageHandlerService;
            _contextAPIGrpcService = contextAPIGrpcService;
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
                if (!await _messageHandlerService.IsFriendAsync(packet.Body.From, packet.Body.To))
                {
                    body.Result = false;
                    body.Message = "好友关系异常";
                    await appSession.SendAsync(resp.Serialize());

                    return;
                }
            }
            var fileinfo = await _contextAPIGrpcService.FetchFileInfoAsync(new FileInfoRequest() { Id = packet.Body.FileId });
            if (fileinfo == null || !fileinfo.Result)
            {
                body.Result = false;
                body.Message = "发送图片失败";
                await appSession.SendAsync(resp.Serialize());

                return;
            }
            _mapper.Map(packet.Body, body);
            body.Result = true;
            body.FileId = fileinfo.Id;
            body.FileName = fileinfo.Name;
            //TODO保存聊天记录到数据库
            //分发给发送者和接收者
            var fromSessions = _sessionManager.TryGet(resp.Body.From);
            var toSessions = _sessionManager.TryGet(resp.Body.To);
            await SendMessageBothAsync<Packet<ImageMessageResponse>>(fromSessions, toSessions, resp.Body.From, resp.Body.To, resp);
        }
    }
}
