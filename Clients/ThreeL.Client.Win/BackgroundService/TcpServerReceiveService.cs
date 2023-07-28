using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using ThreeL.Shared.SuperSocket.Client;

namespace ThreeL.Client.Win.BackgroundService
{
    public class TcpServerReceiveService : IHostedService
    {
        private readonly TcpSuperSocketClient _tcpSuperSocket; //通讯服务器socket
        public TcpServerReceiveService(TcpSuperSocketClient tcpSuperSocket)
        {
            _tcpSuperSocket = tcpSuperSocket;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                var packet = await _tcpSuperSocket.mClient.ReceiveAsync();
                //TODO:处理接收到的数据 将收发放到packet外层处理
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
