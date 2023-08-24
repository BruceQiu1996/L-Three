using SuperSocket;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Commands;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.Shared.SuperSocket.Metadata;
using ThreeL.SocketServer.Application.Contract;
using ThreeL.SocketServer.Application.Contract.Services;

namespace ThreeL.SocketServer.SuperSocketHandlers
{
    public class AddFriendCommandHandler : AbstractMessageHandler
    {
        private readonly ServerAppSessionManager<ChatSession> _sessionManager;
        private readonly IContextAPIGrpcService _contextAPIGrpc;

        public AddFriendCommandHandler(ServerAppSessionManager<ChatSession> sessionManager,
                                   IContextAPIGrpcService contextAPIGrpc) : base(MessageType.AddFriend)
        {
            _sessionManager = sessionManager;
            _contextAPIGrpc = contextAPIGrpc;
        }

        public async override Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var session = appSession as ChatSession;
            var packet = message as Packet<AddFriendCommand>;
            var respPacket = new Packet<AddFriendCommandResponse>()
            {
                MessageType = MessageType.AddFriendResponse,
                Sequence = packet.Sequence,
                Body = new AddFriendCommandResponse()
                {
                    Result = true
                }
            };
            var resp = await _contextAPIGrpc.AddFriendAsync(new AddFriendRequest()
            {
                FriendId = packet.Body.FriendId
            }, session.AccessToken);

            respPacket.Body.Result = resp.Result;
            respPacket.Body.Message = resp.Message;
            respPacket.Body.From = session.UserId;
            respPacket.Body.To = packet.Body.FriendId;

            await appSession.SendAsync(respPacket.Serialize());
            if (resp.Result)//成功后还需要发通知给目标
            {
                var toSessions = _sessionManager.TryGet(packet.Body.FriendId);
                await SendMessageBothAsync<Packet<ImageMessageResponse>>(null, toSessions, 0, packet.Body.FriendId, respPacket);
            }
        }

        public async override Task ExceptionAsync(IAppSession appSession, IPacket message, Exception ex)
        {
            var packet = message as Packet<LoginCommand>;
            var resp = new Packet<LoginCommandResponse>()
            {
                Sequence = packet.Sequence,
                Checkbit = packet.Checkbit,
                MessageType = MessageType.LoginResponse,
            };

            var body = new LoginCommandResponse();
            resp.Body = body;
            body.Result = false;
            body.Message = ex.Message;
            await appSession.SendAsync(resp.Serialize());
        }
    }
}
