using ThreeL.Infra.Repository.Entities.Mongo;

namespace ThreeL.ContextAPI.Domain.Entities
{
    public class JwtSetting : MongoEntity
    {
        public string Section { get; set; }
        public string SecretKey { get; set; }
        public string Issuer { get; set; } //哪个节点颁发的,分布式下可以使用组的概念来区分，或者不验证发布者
        public string[] Audiences { get; set; } //订阅者们
        public DateTime SecretExpireAt { get; set; }
        public int TokenExpireSeconds { get; set; }
        public int ClockSkew { get; set; }
    }
}
