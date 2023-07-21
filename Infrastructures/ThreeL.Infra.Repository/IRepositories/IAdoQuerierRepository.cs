using System.Data;

namespace ThreeL.Infra.Repository.IRepositories
{
    public interface IAdoQuerierRepository<out TDbContext> : IAdoRepository
    {
        Task<IEnumerable<dynamic>?> QueryAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<IEnumerable<T>?> QueryAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<T> QueryFirstAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<T> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
    }
}
