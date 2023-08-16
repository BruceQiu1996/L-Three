using SuperSocket.Channel;
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
        private readonly PackageFilter _packageFilter;
        private readonly MessageHandlerDispatcher _messageHandlerDispatcher;
        public Func<CloseEventArgs, Task> DisConnectionEvent { get; set; }
        Semaphore _semaphore = new Semaphore(1, 1);
        public Action ConnectedEvent { get; set; }

        public TcpSuperSocketClient(PackageFilter packageFilter,
                                    MessageHandlerDispatcher messageHandlerDispatcher)
        {
            _packageFilter = packageFilter;
            _messageHandlerDispatcher = messageHandlerDispatcher;
            Initialize();
        }

        public void Initialize()
        {
            mClient = new EasyClient<IPacket>(_packageFilter).AsClient();
            mClient.Closed += (s, e) =>
            {
                Connected = false;
                Initialize();
                DisConnectionEvent?.Invoke(e as CloseEventArgs);
            };

            mClient.PackageHandler += async (sender, package) =>
            {
                await _messageHandlerDispatcher.DispatcherMessageHandlerAsync(null, package);
            };
        }

        public async Task<bool> ConnectAsync(string remoteIP, int remotePort, int retryTimes = 3)
        {
            _semaphore.WaitOne();
            if (Connected)
                return true;

            try
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
                if (Connected)
                {
                    ConnectedEvent?.Invoke();
                }
                return isConnect;
            }
            finally
            {
                _semaphore.Release();
            }
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

        public async Task<bool> SendBytesAsync(byte[] data)
        {
            try
            {
                if (!Connected)
                {
                    throw new Exception("连接已经断开!");
                }
                await mClient.SendAsync(data);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
