using System.Data;

namespace ThreeL.Infra.Repository.IRepositories
{
    public interface IAdoRepository : IRepository
    {
        bool HasDbConnection();
    }
}
