using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ThreeL.Shared.Application.Contract.Extensions;
using ThreeL.Shared.Application.Contract.Interceptors;
using ThreeL.SocketServer.Application.Contract.Configurations;
using ThreeL.SocketServer.Application.Contract.Interceptors;

namespace ThreeL.SocketServer.Application.Contract.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddSocketServerApplicationService(this IServiceCollection services, IConfiguration configuration, Assembly contractAssembly)
        {
            services.Configure<ContextAPIOptions>(configuration.GetSection("ContextAPIOptions"));
            services.AddApplicationService(configuration, contractAssembly);
        }

        public static void AddSocketServerApplicationContainer(this ContainerBuilder container, Assembly implAssembly)
        {
            container.RegisterType<GrpcExceptionAsyncInterceptor>();
            container.AddApplicationContainer(implAssembly, new List<Type> { typeof(AsyncInterceptorAdaper<GrpcExceptionAsyncInterceptor>) });
        }
    }
}
