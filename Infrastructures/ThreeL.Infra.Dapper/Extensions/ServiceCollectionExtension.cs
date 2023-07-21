using Microsoft.Extensions.DependencyInjection;
using ThreeL.Infra.Dapper.Repositories;
using ThreeL.Infra.Repository.IRepositories;

namespace ThreeL.Infra.Dapper.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddInfraDapper(this IServiceCollection services)
        {
            services.AddScoped(typeof(IAdoQuerierRepository<>), typeof(DapperRepository<>));
            services.AddScoped(typeof(IAdoExecuterRepository<>), typeof(DapperRepository<>));
            services.AddSingleton<DbConnectionFactory>();

            return services;
        }
    }
}
