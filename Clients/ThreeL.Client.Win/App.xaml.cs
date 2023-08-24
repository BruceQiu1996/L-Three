using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using ThreeL.Client.Shared;
using ThreeL.Client.Shared.Entities;
using ThreeL.Client.Win.BackgroundService;
using ThreeL.Client.Win.Handlers;
using ThreeL.Client.Win.Helpers;
using ThreeL.Client.Win.Pages;
using ThreeL.Client.Win.ViewModels;
using ThreeL.Shared.SuperSocket.Extensions;
using ThreeL.Shared.SuperSocket.Handlers;

namespace ThreeL.Client.Win
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static IServiceProvider? ServiceProvider;
        internal static UserProfile UserProfile;
        internal static IHost host;

        protected async override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            var builder = Host.CreateDefaultBuilder(e.Args);
            builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.ConfigureServices((context, service) =>
            {
                service.AddClientShared(context.Configuration);
                service.AddSingleton<MainWindow>();
                service.AddSingleton<MainWindowViewModel>();
                service.AddSingleton<Login>();
                service.AddSingleton<LoginWindowViewModel>();
                service.AddSingleton<Chat>();
                service.AddSingleton<ChatViewModel>();
                service.AddSingleton<Setting>();
                service.AddSingleton<SettingViewModel>();
                service.AddSingleton<Apply>();
                service.AddSingleton<ApplyViewModel>();
                service.AddSingleton<GrowlHelper>();
                service.AddSingleton<FileHelper>();
                service.AddSingleton<DateTimeHelper>();
                service.AddSuperSocket(true);
                service.AddSingleton<SaveChatRecordService>();
                //service.AddHostedService<UdpServerRunningService>();
                service.AddSingleton<CustomerSettings>();
                //message handlers
                service.AddSingleton<IMessageHandler, TextMessageResponseHandler>();
                service.AddSingleton<IMessageHandler, ImageMessageResponseHandler>();
                service.AddSingleton<IMessageHandler, FileMessageResponseHandler>();
                service.AddSingleton<IMessageHandler, LoginCommandResponseHandler>();
                service.AddSingleton<IMessageHandler, WithdrawMessageResponseHandler>();
                service.AddSingleton<IMessageHandler, RequestForUserEndpointResponseHandler>();
                service.AddSingleton<IMessageHandler, AddFriendCommandResponseHandler>();
            }).ConfigureLogging((hostCtx, loggingBuilder) =>
            {
                loggingBuilder.AddConsole();
            });

            builder.ConfigureHostConfiguration(options =>
            {
                options.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            });

            host = builder.Build();
            ServiceProvider = host.Services;
            await host.StartAsync();
            host.Services.GetRequiredService<Login>().Show();
        }

        public async static Task CloseAsync()
        {
            await host.StopAsync();
            host.Dispose();
            Environment.Exit(0);
        }
    }
}
