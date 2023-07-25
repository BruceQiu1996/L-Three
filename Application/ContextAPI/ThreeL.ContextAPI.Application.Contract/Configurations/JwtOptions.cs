namespace ThreeL.ContextAPI.Application.Contract.Configurations
{
    public class JwtOptions
    {
        public string Section { get; set; }
        public string SecretKey { get; set; }
        public string Issuer { get; set; } //哪个节点颁发的,分布式下可以使用组的概念来区分，或者不验证发布者
        public string[] Audiences { get; set; } //订阅者们
        public int ExpireSeconds { get; set; }
        public int ClockSkew { get; set; }
    }
}
