using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ThreeL.Shared.Application.Contract.Extensions;
using ThreeL.Shared.SuperSocket.Extensions;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.SocketServer.Application.Contract.Extensions;
using ThreeL.SocketServer.Application.Impl;
using ThreeL.SocketServer.BackgroundService;
using ThreeL.SocketServer.SuperSocketHandlers;

namespace ThreeL.SocketServer
{
    internal class Program
    {
        internal static IServiceProvider ServiceProvider { get; private set; }
        async static Task Main(string[] args)
        {
            AppAssemblyInfo appAssemblyInfo = new AppAssemblyInfo();
            var builder = Host.CreateDefaultBuilder(args);
            builder.UseServiceProviderFactory(new AutofacServiceProviderFactory()).ConfigureContainer<ContainerBuilder>(cbuilder =>
            {
                cbuilder.AddSocketServerApplicationContainer(appAssemblyInfo.ImplementAssembly);
            });

            builder.ConfigureServices((context, service) =>
            {
                service.AddSocketServerApplicationService(context.Configuration, appAssemblyInfo.ContractAssembly);

                service.AddSingleton<SaveChatRecordService>();
                service.AddSingleton<IMessageHandler, TextMessageHandler>();
                service.AddSingleton<IMessageHandler, ImageMessageHandler>();
                service.AddSingleton<IMessageHandler, FileMessageHandler>();
                service.AddSingleton<IMessageHandler, WithdrawMessageHandler>();
                service.AddSingleton<IMessageHandler, LoginCommandHandler>();
                service.AddSingleton<IMessageHandler, RequestForUserEndpointHandler>();
                service.AddSuperSocket();
                service.AddSingleton<ServerAppSessionManager<ChatSession>>();
                service.AddHostedService<TcpServerRunningService>();
            });
            var host = builder.Build(); ServiceProvider = host.Services;
            await host.PreheatService();
            await host.RunAsync();
        }
    }
}