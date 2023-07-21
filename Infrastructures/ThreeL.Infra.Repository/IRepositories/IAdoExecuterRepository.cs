using System.Data;

namespace ThreeL.Infra.Repository.IRepositories
{
    public interface IAdoExecuterRepository<out TDbContext> : IAdoRepository
    {
        Task<IDataReader> ExecuteReaderAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<object> ExecuteScalarAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<T> ExecuteScalarAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
    }
}
