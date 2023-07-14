using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ThreeL.Shared.SuperSocket.Extensions;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.SocketServer.BackgroundService;
using ThreeL.SocketServer.SuperSocketHandlers;

namespace ThreeL.SocketServer
{
    internal class Program
    {
        async static Task Main(string[] args)
        {

            var builder = Host.CreateDefaultBuilder(args);
            builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.ConfigureServices((context, service) =>
            {
                service.AddSuperSocket(new List<IMessageHandler>() 
                {
                    new TextMessageHandler()
                });

                service.AddHostedService<TcpServerRunningService>();
                service.AddHostedService<GlobalTcpManageService>();
            });
            var host = builder.Build();

            await host.RunAsync();
        }
    }
}