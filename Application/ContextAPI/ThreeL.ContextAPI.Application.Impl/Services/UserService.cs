using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ThreeL.ContextAPI.Application.Contract.Configurations;
using ThreeL.ContextAPI.Application.Contract.Dtos.User;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.ContextAPI.Domain.Aggregates.UserAggregate;
using ThreeL.ContextAPI.Domain.Entities;
using ThreeL.Infra.Redis;
using ThreeL.Infra.Repository.IRepositories;
using ThreeL.Shared.Application.Contract.Configurations;
using ThreeL.Shared.Application.Contract.Helpers;

namespace ThreeL.ContextAPI.Application.Impl.Services
{
    public class UserService : IAppService, IUserService
    {
        private readonly IAdoQuerierRepository<ContextAPIDbContext> _adoQuerierRepository;
        private readonly IAdoExecuterRepository<ContextAPIDbContext> _adoExecuterRepository;
        private readonly IRedisProvider _redisProvider;
        private readonly PasswordHelper _passwordHelper;
        private readonly JwtOptions _jwtOptions;
        private readonly SystemOptions _systemOptions;
        private readonly IMapper _mapper;

        public UserService(IAdoQuerierRepository<ContextAPIDbContext> adoQuerierRepository,
                           IAdoExecuterRepository<ContextAPIDbContext> adoExecuterRepository,
                           PasswordHelper passwordHelper,
                           IRedisProvider redisProvider,
                           IOptions<JwtOptions> jwtOptions,
                           IOptions<SystemOptions> systemOptions,
                           IMapper mapper)
        {
            _mapper = mapper;
            _passwordHelper = passwordHelper;
            _redisProvider = redisProvider;
            _systemOptions = systemOptions.Value;
            _jwtOptions = jwtOptions.Value;
            _adoQuerierRepository = adoQuerierRepository;
            _adoExecuterRepository = adoExecuterRepository;
        }

        public async Task CreateUserAsync(UserCreationDto creationDto, long creator)
        {
            creationDto.Password = _passwordHelper.HashPassword(creationDto.Password);
            var user = _mapper.Map<User>(creationDto);
            user.CreateTime = DateTime.Now;
            user.CreateBy = creator;
            await _adoExecuterRepository.ExecuteScalarAsync<User>
                ("INSERT INTO [User] (userName,password,isDeleted,createBy,createTime,role) VALUES (@UserName, @Password, 0, @CreateBy, @CreateTime, @Role)",
                new { creationDto.UserName, creationDto.Password, creationDto.Role, user.CreateBy, user.CreateTime });
        }

        public async Task<UserLoginResponseDto> LoginAsync(UserLoginDto userLoginDto)
        {
            var user = await _adoQuerierRepository.QueryFirstOrDefaultAsync<User>("SELECT * FROM [User] WHERE userName = @UserName AND isDeleted = 0",
                new { userLoginDto.UserName, userLoginDto.Password }); ;

            if (user == null)
                return null;

            if (!_passwordHelper.VerifyHashedPassword(userLoginDto.Password, user.Password))
            {
                return null;
            }
            else
            {
                var setting = await _redisProvider.HGetAsync<JwtSetting>(Const.REDIS_JWT_SECRET_KEY, _systemOptions.Name);
                var token = CreateToken(user, setting.SecretKey);

                return new UserLoginResponseDto() { Token = token };
            }
        }

        private string CreateToken(User user, string key)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, "admin"),
            };

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var algorithm = SecurityAlgorithms.HmacSha256;
            var signingCredentials = new SigningCredentials(secretKey, algorithm);
            var jwtSecurityToken = new JwtSecurityToken(
                _jwtOptions.Issuer,             //Issuer
                "win",          //Audience TODO 客户端携带客户端类型头
                claims,                          //Claims,
                DateTime.Now,                    //notBefore
                DateTime.Now.AddSeconds(_jwtOptions.ExpireSeconds),    //expires
                signingCredentials               //Credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }
    }
}
