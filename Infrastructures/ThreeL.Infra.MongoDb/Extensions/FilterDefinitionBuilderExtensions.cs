using MongoDB.Driver;
using ThreeL.Infra.Repository.Entities.Mongo;

namespace ThreeL.Infra.MongoDb.Extensions
{
    public static class FilterDefinitionBuilderExtensions
    {
        public static FilterDefinition<TEntity> IdEq<TEntity>(this FilterDefinitionBuilder<TEntity> filter, string id)
            where TEntity : MongoEntity
            => filter.Eq(x => x.Id, id);
    }
}
