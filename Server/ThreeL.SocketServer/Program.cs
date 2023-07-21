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
        internal static IServiceProvider ServiceProvider { get; private set; }
        async static Task Main(string[] args)
        {

            var builder = Host.CreateDefaultBuilder(args);
            builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.ConfigureServices((context, service) =>
            {
                service.AddSingleton<IMessageHandler, TextMessageHandler>();
                service.AddSingleton<IMessageHandler, LoginCommandHandler>();
                service.AddSuperSocket();
                service.AddSingleton<ServerAppSessionManager<ChatSession>>();
                service.AddHostedService<TcpServerRunningService>();
            });
            var host = builder.Build();
            ServiceProvider = host.Services;
            await host.RunAsync();
        }
    }
}