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
    public class FileMessageHandler : AbstractMessageHandler
    {
        private readonly IContextAPIGrpcService _contextAPIGrpcService;
        private readonly IMessageHandlerService _messageHandlerService;
        private readonly SaveChatRecordService _saveChatRecordService;
        private readonly ServerAppSessionManager<ChatSession> _sessionManager;
        private readonly IRedisProvider _redisProvider;
        private readonly IMapper _mapper;
        public FileMessageHandler(ServerAppSessionManager<ChatSession> sessionManager,
                                  IRedisProvider redisProvider,
                                  SaveChatRecordService saveChatRecordService,
                                  IContextAPIGrpcService contextAPIGrpcService,
                                  IMapper mapper,
                                  IMessageHandlerService messageHandlerService) : base(MessageType.File)
        {
            _mapper = mapper;
            _redisProvider = redisProvider;
            _sessionManager = sessionManager;
            _saveChatRecordService = saveChatRecordService;
            _messageHandlerService = messageHandlerService;
            _contextAPIGrpcService = contextAPIGrpcService;
        }

        public async override Task ExceptionAsync(IAppSession appSession, IPacket message, Exception ex)
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
            body.Result = false;
            body.Message = "服务器异常";
            await appSession.SendAsync(resp.Serialize());
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
            var request = _mapper.Map<ChatRecordPostRequest>(body);
            request.MessageRecordType = (int)MessageRecordType.File;
            //await _saveChatRecordService.WriteRecordAsync(request); 异步的可能存在客户端和服务端间隔问题
            var result = await _contextAPIGrpcService.PostChatRecordAsync(request);//还是先使用rpc
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
