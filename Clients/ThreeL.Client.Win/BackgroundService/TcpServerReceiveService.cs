using Microsoft.Extensions.Hosting;
using SuperSocket.Client;
using System.Threading;
using System.Threading.Tasks;
using ThreeL.Shared.SuperSocket.Client;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Handlers;

namespace ThreeL.Client.Win.BackgroundService
{
    public class TcpServerReceiveService : IHostedService
    {
        private readonly TcpSuperSocketClient _tcpSuperSocket; //通讯服务器socket
        private readonly MessageHandlerDispatcher _handlerDispatcher;
        private bool _isStartedReceive = false;
        public TcpServerReceiveService(TcpSuperSocketClient tcpSuperSocket,
                                       MessageHandlerDispatcher handlerDispatcher)
        {
            _handlerDispatcher = handlerDispatcher;
            _tcpSuperSocket = tcpSuperSocket;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                await Task.Delay(1000);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
