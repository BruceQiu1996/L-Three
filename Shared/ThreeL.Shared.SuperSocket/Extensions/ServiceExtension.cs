using Microsoft.Extensions.DependencyInjection;
using ThreeL.Shared.SuperSocket.Client;
using ThreeL.Shared.SuperSocket.Filters;
using ThreeL.Shared.SuperSocket.Handlers;

namespace ThreeL.Shared.SuperSocket.Extensions
{
    public static class ServiceExtension
    {
        public static void AddSuperSocket(this IServiceCollection service,IEnumerable<IMessageHandler> handlers,bool isClient = false) 
        {
            service.AddSingleton(handlers);
            service.AddSingleton(new PackageFilter());
            service.AddSingleton<MessageHandlerDispatcher>();
            if (isClient) 
            {
                service.AddTransient<TcpSuperSocketClient>();
                service.AddTransient<UdpSuperSocketClient>();
            }
        }
    }
}
