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
    public class ReplyAddFriendCommandHandler : AbstractMessageHandler
    {
        private readonly ServerAppSessionManager<ChatSession> _sessionManager;
        private readonly IContextAPIGrpcService _contextAPIGrpc;

        public ReplyAddFriendCommandHandler(ServerAppSessionManager<ChatSession> sessionManager,
                                            IContextAPIGrpcService contextAPIGrpc) : base(MessageType.ReplyAddFriend)
        {
            _sessionManager = sessionManager;
            _contextAPIGrpc = contextAPIGrpc;
        }

        public async override Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var session = appSession as ChatSession;
            var packet = message as Packet<ReplyAddFriendCommand>;
            var respPacket = new Packet<ReplyAddFriendCommandResponse>()
            {
                MessageType = MessageType.ReplyAddFriendResponse,
                Sequence = packet.Sequence,
                Body = new ReplyAddFriendCommandResponse()
                {
                    Result = true
                }
            };
            var resp = await _contextAPIGrpc.ReplyAddFriendAsync(new ReplyAddFriendRequest()
            {
                RequestId = packet.Body.ApplyId,
                Agree = packet.Body.Agree
            }, session.AccessToken);

            respPacket.Body.Result = resp.Result;
            respPacket.Body.Message = resp.Message;
            respPacket.Body.From = resp.ActiverId;
            respPacket.Body.To = resp.PassiverId;
            respPacket.Body.FromName = resp.ActiverName;
            respPacket.Body.ToName = resp.PassiverName;
            respPacket.Body.FromAvatar = resp.ActiverAvatarId;
            respPacket.Body.ToAvatar = resp.PassiverAvatarId;
            respPacket.Body.Agree = packet.Body.Agree;

            if (resp.Result)
            {
                var fromSessions = _sessionManager.TryGet(respPacket.Body.From);
                var toSessions = _sessionManager.TryGet(respPacket.Body.To);
                await SendMessageBothAsync(fromSessions, toSessions, respPacket.Body.From, respPacket.Body.To, respPacket);
            }
            else
            {
                await appSession.SendAsync(respPacket.Serialize());
            }
        }

        public async override Task ExceptionAsync(IAppSession appSession, IPacket message, Exception ex)
        {
            var packet = message as Packet<ReplyAddFriendCommand>;
            var resp = new Packet<ReplyAddFriendCommandResponse>()
            {
                Sequence = packet.Sequence,
                Checkbit = packet.Checkbit,
                MessageType = MessageType.LoginResponse,
            };

            var body = new ReplyAddFriendCommandResponse();
            resp.Body = body;
            body.Result = false;
            body.Message = ex.Message;
            await appSession.SendAsync(resp.Serialize());
        }
    }
}
