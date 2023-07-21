using ThreeL.Infra.Repository.Entities;

namespace ThreeL.Shared.Domain.Entities
{
    public abstract class AggregateRoot<TKey> : DomainEntity<TKey>, IEntity<TKey>
    {
        // TODO event publiser
    }
}
