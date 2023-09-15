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

namespace ThreeL.SocketServer.SuperSocketHandlers
{
    public class FinishVoiceChatMessageHandler : AbstractMessageHandler
    {
        private readonly ServerAppSessionManager<ChatSession> _sessionManager;
        private readonly IContextAPIGrpcService _contextAPIGrpc;
        private readonly IMessageHandlerService _messageHandlerService;
        private readonly IMapper _mapper;

        public FinishVoiceChatMessageHandler(ServerAppSessionManager<ChatSession> sessionManager,
                                               IContextAPIGrpcService contextAPIGrpc,
                                               IMapper mapper,
                                               IMessageHandlerService messageHandlerService) : base(MessageType.FinishVoiceChat)
        {
            _mapper = mapper;
            _sessionManager = sessionManager;
            _contextAPIGrpc = contextAPIGrpc;
            _messageHandlerService = messageHandlerService;
        }

        //TODO利用chatkey查找to
        public async override Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var session = appSession as ChatSession;
            var packet = message as Packet<FinishVoiceChatMessage>;
            var respPacket = new Packet<VoiceChatStatusResponse>()
            {
                MessageType = MessageType.VoiceChatEventResponse,
                Sequence = packet.Sequence,
            };

            var respBody = new VoiceChatStatusResponse()
            {
                From = session.UserId,
                FromName = session.UserName,
                To = packet.Body.To,
            };

            respPacket.Body = respBody;
            if (session.UserId != packet.Body.To)
            {
                if (!await _messageHandlerService.IsValidRelationAsync(session.UserId, packet.Body.To, false, session.AccessToken))
                {
                    respBody.Result = false;
                    respBody.Message = "关系异常";
                    await appSession.SendAsync(respPacket.Serialize());

                    return;
                }
            }

            var fromSessions = _sessionManager.TryGet(session.UserId);
            var toSessions = _sessionManager.TryGet(packet.Body.To);
            var statusResp = await _contextAPIGrpc.UpdateVoiceChatStatus(new VoiceChatRecorStatusUpdateRequest()
            {
                ChatKey = packet.Body.Chatkey,
                Status = (int)packet.Body.Action
            }, session.AccessToken);

            if (statusResp != null && statusResp.Result)
            {
                //清空sessions的chatkey TODO使用领域的方式
                //事件
                respBody.ChatKey = packet.Body.Chatkey;
                respBody.Event = (VoiceChatStatus)statusResp.Status;
                await SendMessageBothAsync(new List<IAppSession>() { session }, toSessions, respPacket.Body.From, respPacket.Body.To, respPacket);
                //消息
                var messagePacket = new Packet<VoiceChatMessageResponse>()
                {
                    MessageType = MessageType.VoiceChat
                };
                var messagePacketBody = new VoiceChatMessageResponse()
                {
                    From = respPacket.Body.From,
                    To = respPacket.Body.To,
                    FromName = respPacket.Body.FromName,
                    Status = (VoiceChatStatus)statusResp.Status
                };
                messagePacket.Body = messagePacketBody;
                var recordRequest = _mapper.Map<ChatRecordPostRequest>(messagePacketBody);
                recordRequest.Message = statusResp.Status.ToString();
                recordRequest.MessageId = packet.Body.Chatkey;
                var postResult = await _contextAPIGrpc.PostChatRecordAsync(recordRequest, session.AccessToken);
                if (postResult.Result)
                {
                    await SendMessageBothAsync(fromSessions, toSessions, respPacket.Body.From, respPacket.Body.To, messagePacket);
                }
            }
        }

        public async override Task ExceptionAsync(IAppSession appSession, IPacket message, Exception ex)
        {
            var packet = message as Packet<FinishVoiceChatMessage>;
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
