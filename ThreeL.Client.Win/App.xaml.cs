using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Windows;
using ThreeL.Client.Win.BackgroundService;
using ThreeL.Client.Win.ViewModels;
using ThreeL.Shared.SuperSocket.Extensions;

namespace ThreeL.Client.Win
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public  partial class App : Application
    {
        internal static IServiceProvider? ServiceProvider;

        protected async override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            var builder = Host.CreateDefaultBuilder(e.Args);
            builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.ConfigureServices((context, service) =>
            {
                service.AddSingleton<MainWindow>();
                service.AddSingleton<MainWindowViewModel>();
                service.AddSuperSocket(true);
                service.AddHostedService<UdpServerRunningService>();

            }).ConfigureLogging((hostCtx, loggingBuilder) =>
            {
                loggingBuilder.AddConsole();
            });
            
            var host = builder.Build();
            ServiceProvider = host.Services;
            host.Services.GetRequiredService<MainWindow>().Show();

            await host.RunAsync();
        }
    }
}
