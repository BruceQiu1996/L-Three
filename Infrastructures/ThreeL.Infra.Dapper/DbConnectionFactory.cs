using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using ThreeL.Infra.Dapper.Configuration;
using ThreeL.Infra.Repository.IRepositories;

namespace ThreeL.Infra.Dapper
{
    public class DbConnectionFactory
    {
        /// <summary>
        /// 获取数据库连接
        /// </summary>
        /// <returns></returns>
        public DbConnection CreateConnection(DbConnectionOptions options)
        {
            DbConnection connection = null;
            connection = options.DatabaseType switch
            {
                DbTypes.MSSQL => new SqlConnection(options.ConnectionString),
                DbTypes.SQLLITE => new SQLiteConnection(options.ConnectionString),
                _ => throw new NotImplementedException("not support database type or database options are invalid"),
            };

            return connection;
        }
    }
}
