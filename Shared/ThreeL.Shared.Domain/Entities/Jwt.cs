using ThreeL.Infra.Repository.Entities.Mongo;

namespace ThreeL.Shared.Domain.Entities
{
    public class Jwt : MongoEntity
    {
        public string Section { get; set; } //节点名称
        public string Issuer { get; set; } //签发者
        public string[] ValidAudiences { get; set; } //订阅者
        public string SecretKey { get; set; } //密钥
        public int TokenExpireSeconds { get; set; } //token过期时间秒
    }
}
