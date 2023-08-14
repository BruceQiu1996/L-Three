using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ThreeL.Client.Shared.Configurations;
using ThreeL.Client.Shared.Database;
using ThreeL.Client.Shared.Services;
using ThreeL.Client.Shared.Utils;

namespace ThreeL.Client.Shared
{
    public static class ServiceExtensions
    {
        public static void AddClientShared(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<SequenceIncrementer>();
            services.Configure<SqliteOptions>(configuration.GetSection("SqliteOptions"));
            services.AddSingleton<ClientSqliteContext>();
            services.Configure<ContextAPIOptions>(configuration.GetSection("ContextAPIOptions"));
            services.AddSingleton<ContextAPIService>();
            services.Configure<SocketServerOptions>(configuration.GetSection("SocketServerOptions"));
            services.AddHttpClient();
            services.AddSingleton<MessageFileLocationMapper>();
        }
    }
}
