using System.Data;

namespace ThreeL.Infra.Dapper
{
    public abstract class DbContext : IDisposable
    {
        ~DbContext() { Dispose(); }
        public abstract IDbConnection DbConnection { get; protected set; }

        public void Dispose()
        {
            DbConnection?.Close();
            DbConnection?.Dispose();
        }
    }
}
