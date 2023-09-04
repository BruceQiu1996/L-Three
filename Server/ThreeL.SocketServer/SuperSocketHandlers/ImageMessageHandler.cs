using AutoMapper;
using SuperSocket;
using ThreeL.Infra.Core.Metadata;
using ThreeL.Infra.Redis;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.Shared.SuperSocket.Metadata;
using ThreeL.SocketServer.Application.Contract;
using ThreeL.SocketServer.Application.Contract.Services;
using ThreeL.SocketServer.BackgroundService;

namespace ThreeL.SocketServer.SuperSocketHandlers
{
    public class ImageMessageHandler : AbstractMessageHandler
    {
        private readonly IContextAPIGrpcService _contextAPIGrpcService;
        private readonly IMessageHandlerService _messageHandlerService;
        private readonly ServerAppSessionManager<ChatSession> _sessionManager;
        private readonly SaveChatRecordService _saveChatRecordService;
        private readonly IRedisProvider _redisProvider;
        private readonly IMapper _mapper;

        public ImageMessageHandler(ServerAppSessionManager<ChatSession> sessionManager,
                                   IRedisProvider redisProvider,
                                   SaveChatRecordService saveChatRecordService,
                                   IContextAPIGrpcService contextAPIGrpcService,
                                   IMapper mapper,
                                   IMessageHandlerService messageHandlerService) : base(MessageType.Image)
        {
            _mapper = mapper;
            _redisProvider = redisProvider;
            _sessionManager = sessionManager;
            _messageHandlerService = messageHandlerService;
            _contextAPIGrpcService = contextAPIGrpcService;
            _saveChatRecordService = saveChatRecordService;
        }

        public async override Task ExceptionAsync(IAppSession appSession, IPacket message, Exception ex)
        {
            var packet = message as Packet<ImageMessage>;
            var resp = new Packet<ImageMessageResponse>()
            {
                Sequence = packet.Sequence,
                Checkbit = packet.Checkbit,
                MessageType = MessageType.ImageResp,
            };

            var body = new ImageMessageResponse();
            _mapper.Map(packet.Body, body);
            resp.Body = body;
            body.Result = false;
            body.Message = ex.Message;
            await appSession.SendAsync(resp.Serialize());
        }

        public override async Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var chatSession = appSession as ChatSession;
            var packet = message as Packet<ImageMessage>;
            var resp = new Packet<ImageMessageResponse>()
            {
                Sequence = packet.Sequence,
                Checkbit = packet.Checkbit,
                MessageType = MessageType.ImageResp,
            };

            var body = new ImageMessageResponse();
            resp.Body = body;
            resp.Body.From = chatSession.UserId;
            if (chatSession.UserId != packet.Body.To)
            {
                if (!await _messageHandlerService.IsValidRelationAsync(chatSession.UserId, packet.Body.To, packet.Body.IsGroup, chatSession.AccessToken))
                {
                    body.Result = false;
                    body.Message = "好友关系异常";
                    await appSession.SendAsync(resp.Serialize());

                    return;
                }
            }
            if (packet.Body.ImageType == ImageType.Local) //本地图片文件
            {
                var fileinfo = await _contextAPIGrpcService.FetchFileInfoAsync(new FileInfoRequest() 
                { 
                    Id = packet.Body.FileId,
                }, (appSession as ChatSession).AccessToken);

                if (fileinfo == null || !fileinfo.Result)
                {
                    body.Result = false;
                    body.Message = "发送图片失败";
                    await appSession.SendAsync(resp.Serialize());

                    return;
                }

                body.FileId = fileinfo.Id;
                body.FileName = fileinfo.Name;
            }
            _mapper.Map(packet.Body, body);
            body.Result = true;
            var request = _mapper.Map<ChatRecordPostRequest>(body);
            request.MessageRecordType = (int)MessageRecordType.Image;
            //await _saveChatRecordService.WriteRecordAsync(request);
            var result = await _contextAPIGrpcService.PostChatRecordAsync(request, (appSession as ChatSession).AccessToken);//还是先使用rpc
            if (result.Result)
            {
                //分发给发送者和接收者
                var fromSessions = _sessionManager.TryGet(resp.Body.From);
                var toSessions = _sessionManager.TryGet(resp.Body.To);
                await SendMessageBothAsync(fromSessions, toSessions, resp.Body.From, resp.Body.To, resp);
            }
            else
            {
                body.Result = false;
                body.Message = "发送文件失败";
                await appSession.SendAsync(resp.Serialize());

                return;
            }
        }
    }
}
