using SuperSocket.Client;
using System.Net;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Filters;

namespace ThreeL.Shared.SuperSocket.Client
{
    public class TcpSuperSocketClient
    {
        public bool Connected { get; private set; } = false;
        public IEasyClient<IPacket> mClient;

        public TcpSuperSocketClient(PackageFilter packageFilter)
        {
            mClient = new EasyClient<IPacket>(packageFilter).AsClient();
        }

        public async Task<bool> ConnectAsync(string remoteIP, int remotePort, int retryTimes = 3)
        {
            bool isConnect = true;
            int i = 0;
            while (!await mClient.ConnectAsync(new IPEndPoint(IPAddress.Parse(remoteIP), remotePort)) && i < retryTimes)
            {
                isConnect = false;
                i++;
                await Task.Delay(1000);
            };

            Connected = isConnect;

            return isConnect;
        }


        public async Task<bool> CloseConnect()
        {
            await mClient.CloseAsync();
            Connected = false;
            return true;
        }

        public async Task SendBytes(byte[] data)
        {
            try
            {
                await mClient.SendAsync(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SuperSocket发送数据异常  {ex.Message}");
            }
        }
    }
}
