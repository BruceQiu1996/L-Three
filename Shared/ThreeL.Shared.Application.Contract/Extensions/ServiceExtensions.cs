using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.Infra.Dapper;
using ThreeL.Infra.Dapper.Extensions;

namespace ThreeL.Shared.Application.Contract.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddApplicationService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddInfraDapper();
        }

        public static void AddApplicationContainer(this ContainerBuilder container, Assembly implAssembly)
        {
            container.RegisterAssemblyTypes(implAssembly).Where(t => typeof(IAppService).IsAssignableFrom(t)).SingleInstance().AsImplementedInterfaces();
            container.RegisterAssemblyTypes(implAssembly).Where(t => typeof(DbContext).IsAssignableFrom(t)).SingleInstance().AsSelf();
        }
    }
}
