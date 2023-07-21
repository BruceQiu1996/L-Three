using Dapper;
using System.Data;
using ThreeL.Infra.Repository.IRepositories;

namespace ThreeL.Infra.Dapper.Repositories
{
    public class DapperRepository<TDbContext> : IAdoExecuterRepository<TDbContext>, IAdoQuerierRepository<TDbContext> where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;

        public DapperRepository(TDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
                => await SqlMapper.ExecuteAsync(_dbContext.DbConnection, sql, param, transaction, commandTimeout, commandType);
        public async Task<IDataReader> ExecuteReaderAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
                => await SqlMapper.ExecuteReaderAsync(_dbContext.DbConnection, sql, param, transaction, commandTimeout, commandType);

        public async Task<object> ExecuteScalarAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
                => await SqlMapper.ExecuteScalarAsync(_dbContext.DbConnection, sql, param, transaction, commandTimeout, commandType);

        public async Task<T> ExecuteScalarAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
                => await SqlMapper.ExecuteScalarAsync<T>(_dbContext.DbConnection, sql, param, transaction, commandTimeout, commandType);

        public bool HasDbConnection() => _dbContext.DbConnection is not null;

        public async Task<IEnumerable<dynamic>?> QueryAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var result = await SqlMapper.QueryAsync(_dbContext.DbConnection, sql, param, transaction, commandTimeout, commandType);
            return result.Any() ? result : null;
        }

        public async Task<IEnumerable<T>?> QueryAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var result = await SqlMapper.QueryAsync<T>(_dbContext.DbConnection, sql, param, transaction, commandTimeout, commandType);
            return result.Any() ? result : null;
        }

        public async Task<T> QueryFirstAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var result = await SqlMapper.QueryFirstAsync<T>(_dbContext.DbConnection, sql, param, transaction, commandTimeout, commandType);
            return result;
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var result = await SqlMapper.QueryFirstOrDefaultAsync<T>(_dbContext.DbConnection, sql, param, transaction, commandTimeout, commandType);
            return result;
        }
    }
}
