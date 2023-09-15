using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using SuperSocket;
using ThreeL.Infra.Core.Metadata;
using ThreeL.Infra.Redis;
using ThreeL.Shared.Application;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.Shared.SuperSocket.Metadata;
using ThreeL.SocketServer.Application.Contract;
using ThreeL.SocketServer.Application.Contract.Services;

namespace ThreeL.SocketServer.SuperSocketHandlers
{
    public class ApplyforVoiceChatMessageHandler : AbstractMessageHandler
    {
        private readonly ServerAppSessionManager<ChatSession> _sessionManager;
        private readonly IContextAPIGrpcService _contextAPIGrpc;
        private readonly IMessageHandlerService _messageHandlerService;
        private readonly IDistributedLocker _distributedLocker;
        private readonly IMapper _mapper;

        public ApplyforVoiceChatMessageHandler(ServerAppSessionManager<ChatSession> sessionManager,
                                               IContextAPIGrpcService contextAPIGrpc,
                                               IDistributedLocker distributedLocker,
                                               IMapper mapper,
                                               IMessageHandlerService messageHandlerService) : base(MessageType.ApplyVoiceChat)
        {
            _mapper = mapper;
            _sessionManager = sessionManager;
            _contextAPIGrpc = contextAPIGrpc;
            _distributedLocker = distributedLocker;
            _messageHandlerService = messageHandlerService;
        }

        public async override Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var session = appSession as ChatSession;
            var packet = message as Packet<ApplyforVoiceChatMessage>;
            var respPacket = new Packet<VoiceChatStatusResponse>()
            {
                MessageType = MessageType.VoiceChatEventResponse,
                Sequence = packet.Sequence,
            };

            var body = new VoiceChatStatusResponse();

            respPacket.Body = body;
            body.From = session.UserId;
            body.FromName = session.UserName;
            if (session.UserId != packet.Body.To)
            {
                if (!await _messageHandlerService.IsValidRelationAsync(session.UserId, packet.Body.To, false, session.AccessToken))
                {
                    body.Result = false;
                    body.Message = "关系异常";
                    await appSession.SendAsync(respPacket.Serialize());

                    return;
                }
            }

            var fromSessions = _sessionManager.TryGet(session.UserId);
            var toSessions = _sessionManager.TryGet(packet.Body.To);
            if (fromSessions.FirstOrDefault(x => !string.IsNullOrEmpty(x.VoiceChatKey)) != null
                || toSessions.FirstOrDefault(x => !string.IsNullOrEmpty(x.VoiceChatKey)) != null)
            {
                body.Result = false;
                body.Message = "对方或您忙线中";
                await appSession.SendAsync(respPacket.Serialize());

                return;
            }

            body.To = packet.Body.To;
            var request = _mapper.Map<VoiceChatRecordPostRequest>(body);
            request.FromPlatform = session.Platform;
            var resp = await _contextAPIGrpc.PostVoiceChatRecordAsync(request, session.AccessToken);
            if (!resp.Result)
            {
                body.Result = false;
                body.Message = "服务器数据异常";
                await appSession.SendAsync(respPacket.Serialize());

                return;
            }

            var chatKey = resp.ChatKey;
            //创建一个通话超时任务
            var _ = Task.Run(async () =>
            {
                await Task.Delay(45 * 1000);
                if (session != null && session.VoiceChatKey == chatKey
                && _sessionManager.TryGet(packet.Body.To).FirstOrDefault(x => x.VoiceChatKey == chatKey) == null)
                {
                    var result = await _contextAPIGrpc.GetVoiceChatStatus(new VoiceChatRecorStatusRequest()
                    {
                        ChatKey = chatKey
                    }, session.AccessToken);

                    if (!result.Started && session.VoiceChatKey == chatKey)
                    {
                        session.VoiceChatKey = null;
                        //更新数据库和发送消息给客户端
                        await _contextAPIGrpc.UpdateVoiceChatStatus(new VoiceChatRecorStatusUpdateRequest()
                        {
                            ChatKey = chatKey,
                            Status = (int)VoiceChatStatus.NotAccept,
                            FromName = session.UserName,
                        }, session.AccessToken);
                        
                        //通知客户端
                        var notAcceptPacket = new Packet<VoiceChatStatusResponse>()
                        {
                            MessageType = MessageType.VoiceChatEventResponse,
                            Sequence = packet.Sequence,
                            Body = new VoiceChatStatusResponse()
                            {
                                ChatKey = chatKey,
                                From = session.UserId,
                                To = respPacket.Body.To,
                                Event = VoiceChatStatus.NotAccept,
                                Result = true
                            }
                        };

                        await SendMessageBothAsync(new List<IAppSession>() { session }, toSessions, respPacket.Body.From, respPacket.Body.To, notAcceptPacket);
                        //发送未接听的消息
                        var messagePacket = new Packet<VoiceChatMessageResponse>() 
                        {
                            MessageType = MessageType.VoiceChat
                        };
                        var messagePacketBody = new VoiceChatMessageResponse()
                        {
                            From = respPacket.Body.From,
                            To = respPacket.Body.To,
                            FromName = respPacket.Body.FromName,
                            Status = VoiceChatStatus.NotAccept
                        };
                        messagePacket.Body = messagePacketBody;
                        var recordRequest = _mapper.Map<ChatRecordPostRequest>(messagePacketBody);
                        recordRequest.Message = VoiceChatStatus.NotAccept.ToString();
                        recordRequest.MessageId = chatKey;
                        var postResult = await _contextAPIGrpc.PostChatRecordAsync(recordRequest, session.AccessToken);
                        if (postResult.Result)
                        {
                            await SendMessageBothAsync(fromSessions, toSessions, respPacket.Body.From, respPacket.Body.To, messagePacket);
                        }
                    }
                }
            });

            session.VoiceChatKey = chatKey;
            respPacket.Body.ChatKey = chatKey;
            await SendMessageBothAsync(new List<IAppSession>() { session }, toSessions, respPacket.Body.From, respPacket.Body.To, respPacket);
        }

        public async override Task ExceptionAsync(IAppSession appSession, IPacket message, Exception ex)
        {
            var packet = message as Packet<ApplyforVoiceChatMessage>;
            var resp = new Packet<VoiceChatStatusResponse>()
            {
                Sequence = packet.Sequence,
                Checkbit = packet.Checkbit,
                MessageType = MessageType.VoiceChatEventResponse,
            };

            var body = new VoiceChatStatusResponse();
            resp.Body = body;
            body.Result = false;
            body.Message = ex.Message;

            await appSession.SendAsync(resp.Serialize());
        }
    }
}
