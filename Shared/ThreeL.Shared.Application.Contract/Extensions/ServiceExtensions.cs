using Autofac;
using Autofac.Extras.DynamicProxy;
using AutoMapper;
using Castle.DynamicProxy;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.Infra.Dapper;
using ThreeL.Infra.Dapper.Extensions;
using ThreeL.Infra.MongoDb;
using ThreeL.Infra.MongoDb.Configuration;
using ThreeL.Infra.MongoDb.Extensions;
using ThreeL.Infra.Redis.Extensions;
using ThreeL.Shared.Application.Contract.Configurations;
using ThreeL.Shared.Application.Contract.Helpers;

namespace ThreeL.Shared.Application.Contract.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddApplicationService(this IServiceCollection services, IConfiguration configuration, Assembly contractAssembly, Assembly implAssembly,
            List<Type> interceptorTypes)
        {
            services.AddInfraDapper();
            var config = configuration.GetSection("MongoOptions").Get<MongoOptions>();
            if (config != null)
            {
                services.AddInfraMongo<MongoContext>(options =>
                {
                    options.ConnectionString = config.ConnectionString;
                    options.PluralizeCollectionNames = config.PluralizeCollectionNames;
                });
            }
            services.AddInfraRedis(configuration);
            services.Configure<SystemOptions>(configuration.GetSection("System"));
            services.AddAutoMapper(contractAssembly.DefinedTypes.Where(t => typeof(Profile).GetTypeInfo().IsAssignableFrom(t.AsType())).Select(t => t.AsType()).ToArray());
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssembly(contractAssembly);
            services.AddSingleton<PasswordHelper>();
            //services.AddAppliactionSerivcesWithInterceptors(contractAssembly, implAssembly, interceptorTypes);
        }

        public static void AddApplicationContainer(this ContainerBuilder container,
                                                   Assembly implAssembly, List<Type> interceptorTypes)
        {
            container.RegisterAssemblyTypes(implAssembly)
                .Where(t => typeof(IAppService).IsAssignableFrom(t)).AsImplementedInterfaces().SingleInstance().EnableInterfaceInterceptors().InterceptedBy(interceptorTypes.ToArray());
            container.RegisterAssemblyTypes(implAssembly)
                .Where(t => typeof(DbContext).IsAssignableFrom(t)).InstancePerDependency().AsSelf().As<DbContext>();
        }

        public static void AddAppliactionSerivcesWithInterceptors(this IServiceCollection services, Assembly contractAssembly, Assembly implAssembly, List<Type> interceptorTypes)
        {
            var appServiceType = typeof(IAppService);
            var serviceTypes = contractAssembly.GetExportedTypes().Where(type => type.IsInterface && type.IsAssignableTo(appServiceType)).ToList();
            serviceTypes.ForEach(serviceType =>
            {
                var implType = implAssembly.ExportedTypes.FirstOrDefault(type => type.IsAssignableTo(serviceType) && !type.IsAbstract);
                if (implType is null)
                    return;

                services.AddScoped(implType);
                services.TryAddSingleton(new ProxyGenerator());
                services.AddScoped(serviceType, provider =>
                {
                    var interfaceToProxy = serviceType;
                    var target = provider.GetService(implType);
                    var interceptors = interceptorTypes.ConvertAll(interceptorType => provider.GetService(interceptorType) as IInterceptor).ToArray();
                    var proxyGenerator = provider.GetService<ProxyGenerator>();
                    var proxy = proxyGenerator!.CreateInterfaceProxyWithTargetInterface(interfaceToProxy, target, interceptors);

                    return proxy;
                });
            });
        }
    }
}
