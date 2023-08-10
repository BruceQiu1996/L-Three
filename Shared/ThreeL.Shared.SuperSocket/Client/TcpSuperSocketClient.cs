using SuperSocket.Client;
using System.Net;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Filters;
using ThreeL.Shared.SuperSocket.Handlers;

namespace ThreeL.Shared.SuperSocket.Client
{
    public class TcpSuperSocketClient
    {
        public bool Connected { get; private set; } = false;
        public IEasyClient<IPacket> mClient;
        private readonly MessageHandlerDispatcher _messageHandlerDispatcher;

        public TcpSuperSocketClient(PackageFilter packageFilter,
                                    IPEndPoint ipEndPoint,
                                    MessageHandlerDispatcher messageHandlerDispatcher)
        {
            _messageHandlerDispatcher = messageHandlerDispatcher;
            mClient = new EasyClient<IPacket>(packageFilter).AsClient();
            mClient.LocalEndPoint = ipEndPoint;
            mClient.Closed += (s, e) =>
            {
                Connected = false;
                throw new Exception("连接已经断开!"); //TODO: 重连,显示断开连接
            };

            mClient.PackageHandler += async (sender, package) =>
            {
                await _messageHandlerDispatcher.DispatcherMessageHandlerAsync(null, package);
            };
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
            }

            Connected = isConnect;
            return isConnect;
        }

        public async Task<bool> CloseConnectAsync()
        {
            if (Connected)
            {
                await mClient.CloseAsync();
                Connected = false;
            }
            return true;
        }

        public async Task SendBytes(byte[] data)
        {
            try
            {
                if (!Connected) 
                {
                    throw new Exception("连接已经断开!");
                }
                await mClient.SendAsync(data);
            }
            catch
            {
                throw;
            }
        }
    }
}
