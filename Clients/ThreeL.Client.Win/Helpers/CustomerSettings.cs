using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThreeL.Client.Shared.Database;

namespace ThreeL.Client.Win.Helpers
{
    public class CustomerSettings
    {
        public string FileSaveFolder { get; set; }
        private readonly ClientSqliteContext _clientSqliteContext;
        private Dictionary<string, string> _settings;

        public CustomerSettings(ClientSqliteContext clientSqliteContext)
        {
            _clientSqliteContext = clientSqliteContext;
        }

        public async Task Initialize()
        {
            _settings = new Dictionary<string, string>(
                await SqlMapper.QueryAsync<KeyValuePair<string, string>>(_clientSqliteContext.dbConnection, "SELECT * FROM Setting"));
        }

        public string GetSetting(string name)
        {
            return _settings.ContainsKey(name) ? _settings[name] : null;
        }

        public async Task<bool> SetSettingAsync(string name, string value)
        {
            try
            {
                await SqlMapper.ExecuteAsync(_clientSqliteContext.dbConnection,
                    "INSERT OR REPLACE INTO Setting (Name, Value) values (@Name,@Value)", new
                    {
                        Name = name,
                        Value = value
                    });

                _settings[name] = value;

                return true;
            }
            catch
            {
                return false;
            }

        }
    }
}
