using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using ThreeL.ContextAPI.Application.Contract.Configurations;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.ContextAPI.Domain.Entities;
using ThreeL.Infra.Redis;
using ThreeL.Infra.Repository.IRepositories;
using ThreeL.Shared.Application.Contract.Configurations;
using ThreeL.Shared.Application.Contract.Services;

namespace ThreeL.ContextAPI.Application.Impl.Services
{
    /// <summary>
    /// 管理jwt secret key
    /// </summary>
    public class JwtService : IJwtService, IPreheatService
    {
        private readonly IOptions<JwtOptions> _jwtOptions;
        private readonly IOptions<SystemOptions> _systemOptions;
        private readonly IMongoRepository<JwtSetting> _mongoRepository;
        private readonly IRedisProvider _redisProvider;
        private readonly IMapper _mapper;

        public JwtService(IOptions<JwtOptions> jwtOptions,
                          IMapper mapper,
                          IOptions<SystemOptions> systemOptions,
                          IRedisProvider redisProvider,
                          IMongoRepository<JwtSetting> mongoRepository)
        {
            _mapper = mapper;
            _jwtOptions = jwtOptions;
            _mongoRepository = mongoRepository;
            _systemOptions = systemOptions;
            _redisProvider = redisProvider;
        }

        public async Task PreheatAsync()
        {
            var jwtSetting = _mapper.Map<JwtSetting>(_jwtOptions.Value);
            jwtSetting.Section = _systemOptions.Value.Name;
            jwtSetting.SecretKey = Guid.NewGuid().ToString();
            jwtSetting.TokenExpireSeconds = _jwtOptions.Value.TokenExpireSeconds;
            jwtSetting.SecretExpireAt = DateTime.Now.AddSeconds(_jwtOptions.Value.SecretExpireSeconds);
            await _mongoRepository.AddAsync(jwtSetting);
            //默认设置jwt secret key每三天过期
            await _redisProvider.HSetAsync(Const.REDIS_JWT_SECRET_KEY, $"{_systemOptions.Value.Name}-{DateTime.Now}", jwtSetting, TimeSpan.FromDays(3), When.Always);
        }

        public IEnumerable<SecurityKey> ValidateIssuerSigningKey(string token, SecurityToken securityToken, string kid, TokenValidationParameters validationParameters)
        {
            var settings = _redisProvider.HGetAllAsync<JwtSetting>(Const.REDIS_JWT_SECRET_KEY).Result;
            foreach (var item in settings.Where(x => x.Value.Issuer == securityToken.Issuer))
            {
                yield return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(item.Value.SecretKey));
            }
        }
    }
}
