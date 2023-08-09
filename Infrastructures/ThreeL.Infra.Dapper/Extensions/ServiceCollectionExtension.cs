using Microsoft.Extensions.DependencyInjection;
using ThreeL.Infra.Dapper.Repositories;
using ThreeL.Infra.Repository.IRepositories;

namespace ThreeL.Infra.Dapper.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddInfraDapper(this IServiceCollection services)
        {
            services.AddTransient(typeof(IAdoQuerierRepository<>), typeof(DapperRepository<>));
            services.AddTransient(typeof(IAdoExecuterRepository<>), typeof(DapperRepository<>));
            services.AddSingleton<DbConnectionFactory>();

            return services;
        }
    }
}
