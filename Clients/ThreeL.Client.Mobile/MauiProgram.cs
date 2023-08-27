using Autofac.Extensions.DependencyInjection;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using ThreeL.Client.Mobile.Helper;
using ThreeL.Client.Mobile.Pages;
using ThreeL.Client.Mobile.Pages.Content;
using ThreeL.Client.Mobile.ViewModels;
using ThreeL.Client.Shared;
using ThreeL.Shared.SuperSocket.Extensions;

namespace ThreeL.Client.Mobile
{
    public static class MauiProgram
    {
        internal static IServiceProvider ServiceProvider { get; private set; }
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>().UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.ConfigureContainer(new AutofacServiceProviderFactory((containerBuilder) =>
            {

            }));

            var a = Assembly.GetExecutingAssembly();
            using var stream = a.GetManifestResourceStream("ThreeL.Client.Mobile.appsettings.json");
            builder.Configuration.AddJsonStream(stream);

            builder.Services.AddClientShared(builder.Configuration);
            builder.Services.AddSuperSocket(true);
            builder.Services.AddSingleton<WarningHelper>();
            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<MainPageViewModel>();
            builder.Services.AddSingleton<Chat>();
            builder.Services.AddSingleton<ChatViewModel>();
            builder.Services.AddSingleton<Setting>();
#if DEBUG
            builder.Logging.AddDebug();
#endif
            var app =  builder.Build();
            ServiceProvider = app.Services;

            return app;
        }
    }
}