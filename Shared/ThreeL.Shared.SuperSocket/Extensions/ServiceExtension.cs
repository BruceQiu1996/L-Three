using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using ThreeL.Shared.SuperSocket.Cache;
using ThreeL.Shared.SuperSocket.Client;
using ThreeL.Shared.SuperSocket.Filters;
using ThreeL.Shared.SuperSocket.Handlers;

namespace ThreeL.Shared.SuperSocket.Extensions
{
    public static class ServiceExtension
    {
        public static void AddSuperSocket(this IServiceCollection service,bool isClient = false) 
        {
            service.AddSingleton<PacketWaitContainer>();
            service.AddSingleton<PacketWaiter>();
            service.AddSingleton(new PackageFilter());
            service.AddSingleton<MessageHandlerDispatcher>();
            if (isClient)
            {
                service.AddSingleton<TcpSuperSocketClient>();
                service.AddSingleton<UdpSuperSocketClient>();
            }
        }
    }
}
