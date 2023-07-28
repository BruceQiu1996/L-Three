using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ThreeL.Shared.Application.Contract.Extensions;
using ThreeL.Shared.Application.Contract.Interceptors;
using ThreeL.SocketServer.Application.Contract.Configurations;

namespace ThreeL.SocketServer.Application.Contract.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddSocketServerApplicationService(this IServiceCollection services, IConfiguration configuration, Assembly contractAssembly)
        {
            services.AddApplicationService(configuration, contractAssembly);
            services.Configure<ContextAPIOptions>(configuration.GetSection("ContextAPIOptions"));
        }

        public static void AddSocketServerApplicationContainer(this ContainerBuilder container, Assembly implAssembly)
        {
            container.AddApplicationContainer(implAssembly);
            container.RegisterType<DapperUowAsyncInterceptor>().AsSelf();
            container.RegisterType<AsyncInterceptorAdaper<DapperUowAsyncInterceptor>>().AsSelf();
        }
    }
}
