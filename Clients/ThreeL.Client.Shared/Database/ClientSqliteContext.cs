using Microsoft.Extensions.Options;
using System.Data;
using System.Data.SQLite;
using ThreeL.Client.Shared.Configurations;

namespace ThreeL.Client.Shared.Database
{
    public class ClientSqliteContext
    {
        private readonly SqliteOptions _sqliteOptions;

        public ClientSqliteContext(IOptions<SqliteOptions> options)
        {
            _sqliteOptions = options.Value;
        }


        private IDbConnection _dbConnection;
        public IDbConnection dbConnection => _dbConnection ?? (_dbConnection = new SQLiteConnection(_sqliteOptions.ConnectionString));
    }
}
