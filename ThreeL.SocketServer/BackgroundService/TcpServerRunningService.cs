using Microsoft.Extensions.Hosting;
using SuperSocket;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Filters;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.SocketServer.SuperSocketHandlers;

namespace ThreeL.SocketServer.BackgroundService
{
    /// <summary>
    /// 启动服务端tcp server的后台服务
    /// </summary>
    public class TcpServerRunningService : IHostedService
    {
        private readonly MessageHandlerDispatcher _handlerDispatcher;
        private IHost? _tcpServerHost;

        public TcpServerRunningService(MessageHandlerDispatcher handlerDispatcher)
        {
            _handlerDispatcher = handlerDispatcher;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _tcpServerHost = SuperSocketHostBuilder
                .Create<IPacket, PackageFilter>().UsePackageHandler(async (session, package) =>
                {
                    await _handlerDispatcher.DispatcherMessageHandlerAsync(session, package);
                }).UseSession<ChatSession>().Build();

            await _tcpServerHost!.RunAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _tcpServerHost!.StopAsync(cancellationToken);
        }
    }
}
