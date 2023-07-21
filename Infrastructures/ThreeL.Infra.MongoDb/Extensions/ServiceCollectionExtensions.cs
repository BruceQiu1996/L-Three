using Microsoft.Extensions.DependencyInjection;
using ThreeL.Infra.MongoDb.Configuration;
using ThreeL.Infra.MongoDb.Repositories;
using ThreeL.Infra.Repository.IRepositories;

namespace ThreeL.Infra.MongoDb.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfraMongo<TContext>(this IServiceCollection services, Action<MongoOptions> configuraton) where TContext : IMongoContext
        {
            services.Configure(configuraton);
            services.AddSingleton(typeof(IMongoContext), typeof(TContext));
            services.AddTransient(typeof(IMongoRepository<>), typeof(MongoRepository<>));
        }
    }
}
