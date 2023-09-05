using SuperSocket;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Commands;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.Shared.SuperSocket.Metadata;
using ThreeL.SocketServer.Application.Contract;
using ThreeL.SocketServer.Application.Contract.Services;

namespace ThreeL.SocketServer.SuperSocketHandlers
{
    public class InviteMembersIntoGroupCommandHandler : AbstractMessageHandler
    {
        private readonly ServerAppSessionManager<ChatSession> _sessionManager;
        private readonly IContextAPIGrpcService _contextAPIGrpc;

        public InviteMembersIntoGroupCommandHandler(ServerAppSessionManager<ChatSession> sessionManager,
                                   IContextAPIGrpcService contextAPIGrpc) : base(MessageType.InviteFriendsIntoGroup)
        {
            _sessionManager = sessionManager;
            _contextAPIGrpc = contextAPIGrpc;
        }

        public async override Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var session = appSession as ChatSession;
            var packet = message as Packet<InviteMembersIntoGroupCommand>;
            var respPacket = new Packet<InviteMembersIntoGroupCommandResponse>()
            {
                MessageType = MessageType.InviteFriendsIntoGroupResponse,
                Sequence = packet.Sequence,
                Body = new InviteMembersIntoGroupCommandResponse()
                {
                    Result = true
                }
            };

            var resp = await _contextAPIGrpc.InviteFriendsIntoGroupAsync(new InviteFriendsIntoGroupRequest()
            {
                Friends = packet.Body.Friends,
                GroupId = packet.Body.GroupId
            }, session.AccessToken);

            respPacket.Body.InviterId = session.UserId;
            respPacket.Body.Result = resp.Result;
            respPacket.Body.Message = resp.Message;
            respPacket.Body.GroupId = resp.GroupId;
            respPacket.Body.GroupName = resp.GroupName;
            respPacket.Body.GroupAvatar = resp.AvatarId == 0 ? null : resp.AvatarId;

            await appSession.SendAsync(respPacket.Serialize());
            if (resp.Result && !string.IsNullOrEmpty(resp.Friends))
            {
                var ids = resp.Friends.Split(",").Select(long.Parse).ToList().Distinct();
                foreach (var id in ids)
                {
                    var toSessions = _sessionManager.TryGet(id);
                    await SendMessageBothAsync(null, toSessions, 0, id, respPacket);
                }
            }
        }

        public async override Task ExceptionAsync(IAppSession appSession, IPacket message, Exception ex)
        {
            var packet = message as Packet<InviteMembersIntoGroupCommand>;
            var resp = new Packet<InviteMembersIntoGroupCommandResponse>()
            {
                Sequence = packet.Sequence,
                Checkbit = packet.Checkbit,
                MessageType = MessageType.InviteFriendsIntoGroupResponse,
            };

            var body = new InviteMembersIntoGroupCommandResponse();
            resp.Body = body;
            body.Result = false;
            body.Message = ex.Message;

            await appSession.SendAsync(resp.Serialize());
        }
    }
}