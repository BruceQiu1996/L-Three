using MongoDB.Driver;
using ThreeL.Infra.Repository.Entities.Mongo;

namespace ThreeL.Infra.MongoDb
{
    public interface IMongoContext
    {
        Task<IMongoCollection<TEntity>> GetCollectionAsync<TEntity>(CancellationToken cancellationToken = default)
            where TEntity : MongoEntity;
    }
}
