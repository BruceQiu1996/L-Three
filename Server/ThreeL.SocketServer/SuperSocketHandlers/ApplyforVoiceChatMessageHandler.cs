using AutoMapper;
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
            var respPacket = new Packet<ApplyforVoiceChatMessageResponse>()
            {
                MessageType = MessageType.ApplyVoiceChatResponse,
                Sequence = packet.Sequence,
            };

            var body = new ApplyforVoiceChatMessageResponse()
            {
                Result = true
            };

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

            var result = await _distributedLocker.LockAsync(string.Format(CommonConst.VOICE_KEY, session.UserId, packet.Body.To), 3600, true);//锁住
            if (!result.Success)
            {
                body.Result = false;
                body.Message = "通话环境异常";
                await appSession.SendAsync(respPacket.Serialize());

                return;
            }

            var chatKey = result.LockValue;
            body.ChatKey = chatKey;
            body.To = packet.Body.To;
            var request = _mapper.Map<VoiceChatRecordPostRequest>(body);
            request.FromPlatform = session.Platform;
            var resp = await _contextAPIGrpc.PostVoiceChatRecordAsync(request, session.AccessToken);
            if (!resp.Result)
            {
                await _distributedLocker.SafedUnLockAsync(string.Format(CommonConst.VOICE_KEY, session.UserId, packet.Body.To), result.LockValue);
                body.Message = "服务器数据异常";
                await appSession.SendAsync(respPacket.Serialize());

                return;
            }

            //创建一个通话超时任务
            var task = new Task(async () =>
            {
                await Task.Delay(60 * 1000);
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
                            Status = (int)VioceChatRecordStatus.NotAccept
                        }, session.AccessToken);
                        //解锁
                        await _distributedLocker.SafedUnLockAsync(string.Format(CommonConst.VOICE_KEY, session.UserId, packet.Body.To), chatKey);
                        //通知客户端
                    }
                }
            });
            session.VoiceChatKey = chatKey
            await SendMessageBothAsync(fromSessions, toSessions, respPacket.Body.From, respPacket.Body.To, respPacket);
        }

        public async override Task ExceptionAsync(IAppSession appSession, IPacket message, Exception ex)
        {
            var packet = message as Packet<ApplyforVoiceChatMessage>;
            var resp = new Packet<ApplyforVoiceChatMessageResponse>()
            {
                Sequence = packet.Sequence,
                Checkbit = packet.Checkbit,
                MessageType = MessageType.ApplyVoiceChatResponse,
            };

            var body = new ApplyforVoiceChatMessageResponse();
            resp.Body = body;
            body.Result = false;
            body.Message = ex.Message;

            await appSession.SendAsync(resp.Serialize());
        }
    }
}
