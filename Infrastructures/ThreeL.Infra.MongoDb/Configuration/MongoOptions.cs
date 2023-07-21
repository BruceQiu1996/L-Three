namespace ThreeL.Infra.MongoDb.Configuration
{
    public class MongoOptions
    {
        /// <summary>
        /// mongodb连接字符串
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;
        public bool PluralizeCollectionNames { get; set; } = true;
    }
}
