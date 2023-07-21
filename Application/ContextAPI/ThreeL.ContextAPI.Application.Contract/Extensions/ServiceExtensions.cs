using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ThreeL.Infra.Dapper.Configuration;
using ThreeL.Shared.Application.Contract.Extensions;

namespace ThreeL.ContextAPI.Application.Contract.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddContextAPIApplicationService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationService(configuration);
            services.Configure<DbConnectionOptions>(configuration.GetSection("ContextAPIDbConnection"));
        }

        public static void AddContextAPIApplicationContainer(this ContainerBuilder container, Assembly implAssembly)
        {
            container.AddApplicationContainer(implAssembly);
        }
    }
}
