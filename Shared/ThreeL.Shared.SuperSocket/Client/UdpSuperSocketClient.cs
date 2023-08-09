using SuperSocket.Client;
using System.Net;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Filters;

namespace ThreeL.Shared.SuperSocket.Client
{
    /// <summary>
    /// UDP服务只做视频和语音
    /// </summary>
    public class UdpSuperSocketClient
    {
        public EasyClient<IPacket> mClient;

        public UdpSuperSocketClient(PackageFilter packageFilter)
        {
            mClient = new EasyClient<IPacket>(packageFilter);
            mClient.LocalEndPoint = new IPEndPoint(IPAddress.Any, 0);
        }

        public async Task SendBytes(IPEndPoint remote, byte[] data)
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
