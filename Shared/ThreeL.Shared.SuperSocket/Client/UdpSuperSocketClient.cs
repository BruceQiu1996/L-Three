using SuperSocket.Client;
using System.Net;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Filters;

namespace ThreeL.Shared.SuperSocket.Client
{
    public class UdpSuperSocketClient
    {
        public EasyClient<IPacket> mClient;

        public UdpSuperSocketClient(PackageFilter packageFilter)
        {
            mClient = new EasyClient<IPacket>(packageFilter);
        }

        public async Task SendBytes(IPEndPoint remote,byte[] data)
        {
            try
            {
                mClient.AsUdp(remote);
                await mClient.AsClient().SendAsync(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SuperSocket发送数据异常  {ex.Message}");
            }
        }
    }
}
