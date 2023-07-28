using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ThreeL.Client.Shared.Configurations;
using ThreeL.Client.Shared.Database;
using ThreeL.Client.Shared.Services;

namespace ThreeL.Client.Shared
{
    public static class ServiceExtensions
    {
        public static void AddClientShared(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SqliteOptions>(configuration.GetSection("SqliteOptions"));
            services.AddSingleton<ClientSqliteContext>();
            services.Configure<ContextAPIOptions>(configuration.GetSection("ContextAPIOptions"));
            services.AddSingleton<ContextAPIService>();
            services.Configure<ContextAPIOptions>(configuration.GetSection("SocketServerOptions"));
        }
    }
}
