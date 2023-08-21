using Autofac;
using ThreeL.Infra.Dapper.Repositories;
using ThreeL.Infra.Repository.IRepositories;

namespace ThreeL.Infra.Dapper.Extensions
{
    public static class ServiceCollectionExtension
    {
        //public static IServiceCollection AddInfraDapper(this IServiceCollection services)
        //{
        //    services.AddScoped(typeof(IAdoQuerierRepository<>), typeof(DapperRepository<>));
        //    services.AddScoped(typeof(IAdoExecuterRepository<>), typeof(DapperRepository<>));
        //    services.AddSingleton<DbConnectionFactory>();

        //    return services;
        //}

        public static void AddInfraDapper(this ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(DapperRepository<>))
                .As(typeof(IAdoQuerierRepository<>)).As(typeof(IAdoExecuterRepository<>)).InstancePerDependency().AsSelf();

            builder.RegisterType<DbConnectionFactory>().SingleInstance();
        }
    }
}
