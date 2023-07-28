namespace ThreeL.SocketServer.Application.Contract.Configurations
{
    /// <summary>
    /// 分布式部署下，可以设置网关的ip和port
    /// </summary>
    public class ContextAPIOptions
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public int MaxRetryAttempts { get; set; }
    }
}
