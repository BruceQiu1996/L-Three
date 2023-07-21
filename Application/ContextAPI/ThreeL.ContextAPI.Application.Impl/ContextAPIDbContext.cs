using Microsoft.Extensions.Options;
using System.Data;
using ThreeL.Infra.Dapper;
using ThreeL.Infra.Dapper.Configuration;

namespace ThreeL.ContextAPI.Application.Impl
{
    public class ContextAPIDbContext : DbContext
    {
        public ContextAPIDbContext(IOptions<DbConnectionOptions> options, DbConnectionFactory connectionFactory)
        {
            DbConnection = connectionFactory.CreateConnection(options.Value);
        }

        public override IDbConnection DbConnection { get; protected set; }
    }
}
