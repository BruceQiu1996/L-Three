using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;
using ThreeL.ContextAPI.Application.Contract.Configurations;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.ContextAPI.Domain.Entities;
using ThreeL.Infra.Core.Serializer;
using ThreeL.Infra.Redis;
using ThreeL.Infra.Repository.IRepositories;
using ThreeL.Shared.Application.Contract.Configurations;
using ThreeL.Shared.Application.Contract.Services;

namespace ThreeL.ContextAPI.Application.Impl.Services
{
    public class JwtService : IJwtService, IAppService, IPreheatService
    {
        private readonly IOptions<JwtOptions> _jwtOptions;
        private readonly IOptions<SystemOptions> _systemOptions;
        private readonly IMongoRepository<JwtSetting> _mongoRepository;
        private readonly IRedisProvider _redisProvider;
        private readonly IMapper _mapper;
        private readonly JsonSerializerOptions _jsonSerializerOptions = SystemTextJson.GetAdncDefaultOptions();

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
            await _mongoRepository.DeleteManyAsync(Builders<JwtSetting>.Filter.Eq(x => x.Section, jwtSetting.Section));
            await _mongoRepository.AddAsync(jwtSetting);
            var jwtSettingJson = JsonSerializer.Serialize(jwtSetting, _jsonSerializerOptions);
            await _redisProvider.HSetAsync(Const.REDIS_JWT_SECRET_KEY, _systemOptions.Value.Name, jwtSettingJson, When.Always);
        }

        public bool ValidateIssuerSigningKey(SecurityKey securityKey, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            var jwtSetting = _mongoRepository.GetAllAsync(Builders<JwtSetting>.Filter.Eq(x => x.Issuer, _jwtOptions.Value.Issuer)).Result;
            foreach (var item in jwtSetting)
            {
                if (securityKey.Equals(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(item.SecretKey))))
                    return true;
            }

            return false;
        }
    }
}
