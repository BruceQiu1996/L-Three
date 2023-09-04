using AutoMapper;
using SuperSocket;
using ThreeL.Infra.Redis;
using ThreeL.Shared.Application;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.Shared.SuperSocket.Metadata;
using ThreeL.SocketServer.Application.Contract;
using ThreeL.SocketServer.Application.Contract.Services;
using ThreeL.SocketServer.BackgroundService;

namespace ThreeL.SocketServer.SuperSocketHandlers
{
    public class TextMessageHandler : AbstractMessageHandler
    {
        private readonly ServerAppSessionManager<ChatSession> _sessionManager;
        private readonly IRedisProvider _redisProvider;
        private readonly IMessageHandlerService _messageHandlerService;
        private readonly SaveChatRecordService _saveChatRecordService;
        private readonly IContextAPIGrpcService _contextAPIGrpcService;
        private readonly IMapper _mapper;
        public TextMessageHandler(ServerAppSessionManager<ChatSession> sessionManager,
                                  IMessageHandlerService messageHandlerService,
                                  IContextAPIGrpcService contextAPIGrpcService,
                                  IMapper mapper,
                                  SaveChatRecordService saveChatRecordService,
                                  IRedisProvider redisProvider) : base(MessageType.Text)
        {
            _mapper = mapper;
            _contextAPIGrpcService = contextAPIGrpcService;
            _redisProvider = redisProvider;
            _sessionManager = sessionManager;
            _messageHandlerService = messageHandlerService;
            _saveChatRecordService = saveChatRecordService;
        }

        public async override Task ExceptionAsync(IAppSession appSession, IPacket message, Exception ex)
        {
            var packet = message as Packet<TextMessage>;
            var resp = new Packet<TextMessageResponse>()
            {
                Sequence = packet.Sequence,
                Checkbit = packet.Checkbit,
                MessageType = MessageType.TextResp,
            };

            var body = new TextMessageResponse();
            _mapper.Map(packet.Body, body);
            resp.Body = body;
            body.Result = false;
            body.Message = ex.Message;
            await appSession.SendAsync(resp.Serialize());
        }

        //TODO 利用消息队列存储消息记录到数据库，可以使用cap,不使用grpc
        public override async Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var chatSession = appSession as ChatSession;
            var packet = message as Packet<TextMessage>;
            var resp = new Packet<TextMessageResponse>()
            {
                Sequence = packet.Sequence,
                Checkbit = packet.Checkbit,
                MessageType = MessageType.TextResp,
            };

            var body = new TextMessageResponse();
            resp.Body = body;
            body.From = chatSession.UserId;
            if (chatSession.UserId != packet.Body.To || packet.Body.IsGroup)
            {
                if (!await _messageHandlerService.IsValidRelationAsync(chatSession.UserId, packet.Body.To, packet.Body.IsGroup, chatSession.AccessToken))
                {
                    body.Result = false;
                    body.Message = "关系异常";
                    await appSession.SendAsync(resp.Serialize());
                    return;
                }
            }
            _mapper.Map(packet.Body, body);
            body.Result = true;
            body.FromName = chatSession.UserName;
            var request = _mapper.Map<ChatRecordPostRequest>(body);
            var result = await _contextAPIGrpcService.PostChatRecordAsync(request, chatSession.AccessToken);
            if (result.Result)
            {
                if (!packet.Body.IsGroup)
                {
                    //分发给发送者和接收者
                    var fromSessions = _sessionManager.TryGet(resp.Body.From);
                    var toSessions = _sessionManager.TryGet(resp.Body.To);
                    await SendMessageBothAsync(fromSessions, toSessions, resp.Body.From, resp.Body.To, resp);
                }
                else
                {
                    var members = await _redisProvider.SetGetAsync(string.Format(CommonConst.GROUP, packet.Body.To));
                    var ids = members.Select(long.Parse).ToList();
                    Parallel.ForEach(ids, async id =>
                    {
                        var toSessions = _sessionManager.TryGet(id);
                        await SendMessageBothAsync(null, toSessions, 0, id, resp);
                    });
                }
            }
            else
            {
                body.Result = false;
                body.Message = "发送消息失败";
                await appSession.SendAsync(resp.Serialize());

                return;
            }
        }
    }
}
