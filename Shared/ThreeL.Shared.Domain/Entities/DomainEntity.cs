using ThreeL.Infra.Repository.Entities;

namespace ThreeL.Shared.Domain.Entities
{
    public class DomainEntity<TKey> : IEntity<TKey>
    {
        public TKey Id { get; set; }
    }
}
