using Dapper;
using System.Data;
using ThreeL.Infra.Repository.IRepositories;

namespace ThreeL.Infra.Dapper.Repositories
{
    public class DapperRepository<TDbContext> : IAdoExecuterRepository, IAdoQuerierRepository where TDbContext : DbContext
    {
        internal TDbContext DbContext { get; private set; }

        public async Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
                => await SqlMapper.ExecuteAsync(DbContext.DbConnection, sql, param, transaction, commandTimeout, commandType);
        public async Task<IDataReader> ExecuteReaderAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
                => await SqlMapper.ExecuteReaderAsync(DbContext.DbConnection, sql, param, transaction, commandTimeout, commandType);

        public async Task<object> ExecuteScalarAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
                => await SqlMapper.ExecuteScalarAsync(DbContext.DbConnection, sql, param, transaction, commandTimeout, commandType);

        public async Task<T> ExecuteScalarAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
                => await SqlMapper.ExecuteScalarAsync<T>(DbContext.DbConnection, sql, param, transaction, commandTimeout, commandType);

        public bool HasDbConnection() => DbContext.DbConnection is not null;

        public async Task<IEnumerable<dynamic>?> QueryAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var result = await SqlMapper.QueryAsync(DbContext.DbConnection, sql, param, transaction, commandTimeout, commandType);
            return result.Any() ? result : null;
        }

        public async Task<IEnumerable<T>?> QueryAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var result = await SqlMapper.QueryAsync<T>(DbContext.DbConnection, sql, param, transaction, commandTimeout, commandType);
            return result.Any() ? result : null;
        }

        public async Task<T> QueryFirstAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var result = await SqlMapper.QueryFirstAsync<T>(DbContext.DbConnection, sql, param, transaction, commandTimeout, commandType);
            return result;
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var result = await SqlMapper.QueryFirstOrDefaultAsync<T>(DbContext.DbConnection, sql, param, transaction, commandTimeout, commandType);
            return result;
        }
    }
}
