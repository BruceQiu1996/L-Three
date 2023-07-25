using Autofac;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ThreeL.ContextAPI.Application.Contract.Configurations;
using ThreeL.Infra.Dapper.Configuration;
using ThreeL.Shared.Application.Contract.Extensions;

namespace ThreeL.ContextAPI.Application.Contract.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddContextAPIApplicationService(this IServiceCollection services, IConfiguration configuration, Assembly contractAssembly)
        {
            services.AddApplicationService(configuration, contractAssembly);
            services.Configure<DbConnectionOptions>(configuration.GetSection("ContextAPIDbConnection"));
            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        }

        public static void AddContextAPIApplicationContainer(this ContainerBuilder container, Assembly implAssembly)
        {
            container.AddApplicationContainer(implAssembly);
        }
    }
}
