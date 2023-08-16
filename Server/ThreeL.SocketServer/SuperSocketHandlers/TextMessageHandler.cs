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
            resp.Body = body;
            body.Result = false;
            body.Message = "服务器异常";
            await appSession.SendAsync(resp.Serialize());
        }

        public override async Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var packet = message as Packet<TextMessage>;
            var resp = new Packet<TextMessageResponse>()
            {
                Sequence = packet.Sequence,
                Checkbit = packet.Checkbit,
                MessageType = MessageType.TextResp,
            };

            var body = new TextMessageResponse();
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
            _mapper.Map(packet.Body, body);
            body.Result = true;
            var request = _mapper.Map<ChatRecordPostRequest>(body);
            request.MessageRecordType = (int)MessageRecordType.Text;
            //await _saveChatRecordService.WriteRecordAsync(request);
            var result = await _contextAPIGrpcService.PostChatRecordAsync(request, (appSession as ChatSession).AccessToken);//还是先使用rpc
            if (result.Result)
            {
                //分发给发送者和接收者
                var fromSessions = _sessionManager.TryGet(resp.Body.From);
                var toSessions = _sessionManager.TryGet(resp.Body.To);
                await SendMessageBothAsync<Packet<ImageMessageResponse>>(fromSessions, toSessions, resp.Body.From, resp.Body.To, resp);
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
