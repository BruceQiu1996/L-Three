using Microsoft.Extensions.Options;
using SuperSocket;
using SuperSocket.Channel;
using SuperSocket.Server;
using ThreeL.Shared.SuperSocket.Dto;

namespace ThreeL.SocketServer.BackgroundService
{
    public class GlobalTcpManageService : SuperSocketService<IPacket>
    {
        public GlobalTcpManageService(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions) : base(serviceProvider, serverOptions)
        {

        }

        protected override async ValueTask OnSessionConnectedAsync(IAppSession session)
        {
            // do something right after the sesssion is connected
            await base.OnSessionConnectedAsync(session);
        }

        protected override ValueTask OnSessionClosedAsync(IAppSession session, CloseEventArgs e)
        {
            return base.OnSessionClosedAsync(session, e);
        }
    }
}
