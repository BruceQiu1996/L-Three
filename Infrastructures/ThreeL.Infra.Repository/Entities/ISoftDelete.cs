namespace ThreeL.Infra.Repository.Entities
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
    }
}
