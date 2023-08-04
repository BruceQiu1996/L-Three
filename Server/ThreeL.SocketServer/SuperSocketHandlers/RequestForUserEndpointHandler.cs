using SuperSocket;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Commands;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.SocketServer.SuperSocketHandlers
{
    public class RequestForUserEndpointHandler : AbstractMessageHandler
    {
        private readonly ServerAppSessionManager<ChatSession> _sessionManager;
        public RequestForUserEndpointHandler(ServerAppSessionManager<ChatSession> sessionManager) : base(MessageType.RequestForUserEndpoint)
        {
            _sessionManager = sessionManager;
        }

        public async override Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var session = appSession as ChatSession;
            var resp = new Packet<RequestForUserEndpointCommandResponse>()
            {
                MessageType = MessageType.RequestForUserEndpointResponse,
                Sequence = message.Sequence,
                Body = new RequestForUserEndpointCommandResponse()
                {
                    Result = false,
                }
            };
            var packet = message as Packet<RequestForUserEndpointCommand>;
            if (packet.Body.SsToken != session.SsToken)
            {
                resp.Body.Message = "Invalid SsToken";
            }
            else
            {
                var sesions = _sessionManager.TryGet(packet.Body.UserId);
                if (sesions == null || sesions.Count() <= 0)
                {
                    resp.Body.Message = "User is not online";
                    resp.Body.Result = false;
                }
                else
                {
                    resp.Body.Addresses = string.Join(",", sesions.Select(x => x.RemoteEndPoint.ToString()));
                    resp.Body.Result = true;
                }
            }

            await appSession.SendAsync(resp.Serialize());
        }
    }
}
