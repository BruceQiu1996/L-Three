using AutoMapper;
using SuperSocket;
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
    public class WithdrawMessageHandler : AbstractMessageHandler
    {
        private readonly IContextAPIGrpcService _contextAPIGrpcService;
        private readonly IMessageHandlerService _messageHandlerService;
        private readonly SaveChatRecordService _saveChatRecordService;
        private readonly ServerAppSessionManager<ChatSession> _sessionManager;
        private readonly IRedisProvider _redisProvider;
        private readonly IMapper _mapper;
        public WithdrawMessageHandler(ServerAppSessionManager<ChatSession> sessionManager,
                                  IRedisProvider redisProvider,
                                  SaveChatRecordService saveChatRecordService,
                                  IContextAPIGrpcService contextAPIGrpcService,
                                  IMapper mapper,
                                  IMessageHandlerService messageHandlerService) : base(MessageType.Withdraw)
        {
            _mapper = mapper;
            _redisProvider = redisProvider;
            _sessionManager = sessionManager;
            _saveChatRecordService = saveChatRecordService;
            _messageHandlerService = messageHandlerService;
            _contextAPIGrpcService = contextAPIGrpcService;
        }

        public async override Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var chatSession = appSession as ChatSession;
            var packet = message as Packet<WithdrawMessage>;
            var resp = new Packet<WithdrawMessageResponse>()
            {
                Sequence = packet.Sequence,
                Checkbit = packet.Checkbit,
                MessageType = MessageType.WithdrawResp,
            };

            var body = _mapper.Map<WithdrawMessageResponse>(packet.Body);
            resp.Body = body;
            resp.Body.From = chatSession.UserId;
            var result = await _contextAPIGrpcService.WithdrawChatRecordAsync(new ChatRecordWithdrawRequest() 
            { 
                MessageId = packet.Body.WithdrawMessageId 
            }, (appSession as ChatSession).AccessToken);

            if (result == null || !result.Result)
            {
                body.Result = false;
                body.Message = "撤回消息失败";
                await appSession.SendAsync(resp.Serialize());

                return;
            }
            else 
            {
                body.Result = true;
                //分发给发送者和接收者
                var fromSessions = _sessionManager.TryGet(resp.Body.From);
                var toSessions = _sessionManager.TryGet(resp.Body.To);
                await SendMessageBothAsync<Packet<ImageMessageResponse>>(fromSessions, toSessions, resp.Body.From, resp.Body.To, resp);
            }
        }

        public async override Task ExceptionAsync(IAppSession appSession, IPacket message, Exception ex)
        {
            var packet = message as Packet<WithdrawMessage>;
            var resp = new Packet<WithdrawMessageResponse>()
            {
                Sequence = packet.Sequence,
                Checkbit = packet.Checkbit,
                MessageType = MessageType.WithdrawResp,
            };
            var body = new WithdrawMessageResponse();
            resp.Body = body;
            body.Result = false;
            body.Message = ex.Message;
            body.WithdrawMessageId = packet.Body.WithdrawMessageId;
            await appSession.SendAsync(resp.Serialize());
        }
    }
}
