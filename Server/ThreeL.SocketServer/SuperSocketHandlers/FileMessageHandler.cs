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
    public class FileMessageHandler : AbstractMessageHandler
    {
        private readonly IContextAPIGrpcService _contextAPIGrpcService;
        private readonly IMessageHandlerService _messageHandlerService;
        private readonly ServerAppSessionManager<ChatSession> _sessionManager;
        private readonly IRedisProvider _redisProvider;
        private readonly IMapper _mapper;
        public FileMessageHandler(ServerAppSessionManager<ChatSession> sessionManager,
                                  IRedisProvider redisProvider,
                                  IContextAPIGrpcService contextAPIGrpcService,
                                  IMapper mapper,
                                  IMessageHandlerService messageHandlerService) : base(MessageType.File)
        {
            _mapper = mapper;
            _redisProvider = redisProvider;
            _sessionManager = sessionManager;
            _messageHandlerService = messageHandlerService;
            _contextAPIGrpcService = contextAPIGrpcService;
        }

        public override async Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var packet = message as Packet<FileMessage>;
            var resp = new Packet<FileMessageResponse>()
            {
                Sequence = packet.Sequence,
                Checkbit = packet.Checkbit,
                MessageType = MessageType.FileResp,
            };

            var body = new FileMessageResponse();
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
                body.Message = "发送文件失败";
                await appSession.SendAsync(resp.Serialize());

                return;
            }

            body.FileId = fileinfo.Id;
            body.FileName = fileinfo.Name;
            body.Size = fileinfo.Size;
            _mapper.Map(packet.Body, body);
            body.Result = true;
            //TODO保存聊天记录到数据库
            //分发给发送者和接收者
            var fromSessions = _sessionManager.TryGet(resp.Body.From);
            var toSessions = _sessionManager.TryGet(resp.Body.To);
            await SendMessageBothAsync<Packet<ImageMessageResponse>>(fromSessions, toSessions, resp.Body.From, resp.Body.To, resp);
        }
    }
}
