using ThreeL.Infra.Repository.IRepositories;

namespace ThreeL.Infra.Dapper.Configuration
{
    public class DbConnectionOptions
    {
        public string? ConnectionString { get; set; }
        public DbTypes DatabaseType { get; set; }
    }
}
