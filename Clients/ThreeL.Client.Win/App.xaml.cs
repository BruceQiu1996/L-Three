using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
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
using ThreeL.Infra.Core.Metadata;
using ThreeL.Infra.Core.Serilog;
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
            SerilogExtension.BuildSerilogLogger(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"), null,
                Module.CLIENT_WIN_THREAD_EXCEPTION,
                Module.CLIENT_WIN_UI_EXCEPTION);
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
                service.AddTransient<VoiceChatWindow>();
                service.AddTransient<VoiceChatWindowViewModel>();
                service.AddTransient<CreateGroupWindow>();
                service.AddTransient<CreateGroupWindowViewModel>();
                service.AddTransient<GroupDetailWindow>();
                service.AddTransient<GroupDetailWindowViewModel>();
                service.AddTransient<InviteFriendsIntoGroup>();
                service.AddTransient<InviteFriendsIntoGroupViewModel>();
                service.AddSingleton<GrowlHelper>();
                service.AddSingleton<FileHelper>();
                service.AddSingleton<CaptureHelper>();
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
                service.AddSingleton<IMessageHandler, ReplyAddFriendCommandResponseHandler>();
                service.AddSingleton<IMessageHandler, InviteMembersIntoGroupResponseHandler>();
                service.AddSingleton<IMessageHandler, OfflineCommandHandler>();
                service.AddSingleton<IMessageHandler, VoiceChatEventResponseHandler>();
                service.AddSingleton<IMessageHandler, VoiceChatMessageResponseHandler>();
            }).UseSerilog();

            builder.ConfigureHostConfiguration(options =>
            {
                options.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            });

            host = builder.Build();
            ServiceProvider = host.Services;
            await host.StartAsync();
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            //非UI线程未捕获异常处理事件(例如自己创建的一个子线程)
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            host.Services.GetRequiredService<Login>().Show();
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            ServiceProvider.GetRequiredService<GrowlHelper>().Warning("未知错误");
            ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(Module.CLIENT_WIN_TASK_EXCEPTION))
                 .LogError(e.ToString());
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ServiceProvider.GetRequiredService<GrowlHelper>().Warning("未知错误");
            ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(Module.CLIENT_WIN_THREAD_EXCEPTION))
                .LogError(e.ToString());
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ServiceProvider.GetRequiredService<GrowlHelper>().Warning("未知错误");
            ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(Module.CLIENT_WIN_UI_EXCEPTION))
                .LogError(e.ToString());
        }

        public async static Task CloseAsync()
        {
            await host.StopAsync();
            host.Dispose();
            Environment.Exit(0);
        }
    }
}
