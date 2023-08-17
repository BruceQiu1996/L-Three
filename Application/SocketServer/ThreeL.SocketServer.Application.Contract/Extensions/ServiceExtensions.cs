using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ThreeL.Shared.Application.Contract.Extensions;
using ThreeL.SocketServer.Application.Contract.Configurations;
using ThreeL.SocketServer.Application.Contract.Interceptors;

namespace ThreeL.SocketServer.Application.Contract.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddSocketServerApplicationService(this IServiceCollection services, IConfiguration configuration, Assembly contractAssembly, Assembly implAssembly)
        {
            services.Configure<ContextAPIOptions>(configuration.GetSection("ContextAPIOptions"));
            services.AddScoped<GrpcExceptionInterceptor>();
            services.AddScoped<GrpcExceptionAsyncInterceptor>();
            services.AddApplicationService(configuration, contractAssembly, implAssembly, new List<Type> { typeof(GrpcExceptionInterceptor) });
        }

        public static void AddSocketServerApplicationContainer(this ContainerBuilder container, Assembly implAssembly)
        {
            container.AddApplicationContainer(implAssembly, new List<Type> { typeof(GrpcExceptionInterceptor) });
        }
    }
}
