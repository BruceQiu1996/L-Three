using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ThreeL.Infra.Dapper.Repositories;
using ThreeL.Infra.Repository.IRepositories;

namespace ThreeL.Infra.Dapper.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddInfraDapper(this IServiceCollection services)
        {
            services.TryAddScoped<IAdoExecuterRepository, DapperRepository<DbContext>>();
            services.TryAddScoped<IAdoQuerierRepository, DapperRepository<DbContext>>();

            return services;
        }
    }
}
