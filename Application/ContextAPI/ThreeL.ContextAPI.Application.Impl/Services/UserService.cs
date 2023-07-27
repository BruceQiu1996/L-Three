using AutoMapper;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ThreeL.ContextAPI.Application.Contract.Configurations;
using ThreeL.ContextAPI.Application.Contract.Dtos.User;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.ContextAPI.Domain.Aggregates.UserAggregate;
using ThreeL.ContextAPI.Domain.Entities;
using ThreeL.Infra.Dapper;
using ThreeL.Infra.Redis;
using ThreeL.Infra.Repository.IRepositories;
using ThreeL.Shared.Application.Contract.Configurations;
using ThreeL.Shared.Application.Contract.Helpers;

namespace ThreeL.ContextAPI.Application.Impl.Services
{
    public class UserService : IAppService, IUserService
    {
        private const string RefreshTokenIdClaimType = "refresh_token_id";
        private readonly IAdoQuerierRepository<ContextAPIDbContext> _adoQuerierRepository;
        private readonly IAdoExecuterRepository<ContextAPIDbContext> _adoExecuterRepository;
        private readonly DbContext _contextAPIDbContext;
        private readonly IRedisProvider _redisProvider;
        private readonly PasswordHelper _passwordHelper;
        private readonly JwtOptions _jwtOptions;
        private readonly JwtBearerOptions _jwtBearerOptions;
        private readonly SystemOptions _systemOptions;
        private readonly IMapper _mapper;

        public UserService(IAdoQuerierRepository<ContextAPIDbContext> adoQuerierRepository,
                           IAdoExecuterRepository<ContextAPIDbContext> adoExecuterRepository,
                           PasswordHelper passwordHelper,
                           IRedisProvider redisProvider,
                           IOptions<JwtOptions> jwtOptions,
                           IOptionsSnapshot<JwtBearerOptions> jwtBearerOptions,
                           DbContext contextAPIDbContext,
                           IOptions<SystemOptions> systemOptions,
                           IMapper mapper)
        {
            _mapper = mapper;
            _passwordHelper = passwordHelper;
            _redisProvider = redisProvider;
            _systemOptions = systemOptions.Value;
            _jwtOptions = jwtOptions.Value;
            _jwtBearerOptions = jwtBearerOptions.Get(JwtBearerDefaults.AuthenticationScheme);
            _adoQuerierRepository = adoQuerierRepository;
            _adoExecuterRepository = adoExecuterRepository;
            _contextAPIDbContext = contextAPIDbContext;
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
                return default;

            if (!_passwordHelper.VerifyHashedPassword(userLoginDto.Password, user.Password))
            {
                return default;
            }
            else
            {
                var token = await CreateTokenAsync(user);
                return new UserLoginResponseDto() { AccessToken = token.accessToken, RefreshToken = token.refreshToken };
            }
        }

        private async Task<(string accessToken, string refreshToken)> CreateTokenAsync(User user)
        {
            var settings = await _redisProvider.HGetAllAsync<JwtSetting>(Const.REDIS_JWT_SECRET_KEY);
            var setting = settings
                .FirstOrDefault(x => x.Key.StartsWith(_systemOptions.Name)
                && x.Value.Issuer == _jwtOptions.Issuer).Value;

            var refreshToken = await CreateRefreshTokenAsync(user.UserName);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(RefreshTokenIdClaimType,refreshToken.refreshTokenId)
            };

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(setting.SecretKey));
            var algorithm = SecurityAlgorithms.HmacSha256;
            var signingCredentials = new SigningCredentials(secretKey, algorithm);
            var jwtSecurityToken = new JwtSecurityToken(
                _jwtOptions.Issuer,             //Issuer
                "win",          //Audience TODO 客户端携带客户端类型头
                claims,                          //Claims,
                DateTime.Now,                    //notBefore
                DateTime.Now.AddSeconds(_jwtOptions.TokenExpireSeconds),    //expires
                signingCredentials               //Credentials
            );

            return (new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken), refreshToken.refreshToken);
        }

        private async Task<(string refreshTokenId, string refreshToken)> CreateRefreshTokenAsync(string userName)
        {
            var tokenId = Guid.NewGuid().ToString("N");
            var rnBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(rnBytes);
            var token = Convert.ToBase64String(rnBytes);
            await _redisProvider.StringSetAsync($"refresh-token:{userName}-{tokenId}", token, TimeSpan.FromSeconds(_jwtOptions.RefreshTokenExpireSeconds));

            return (tokenId, token);
        }

        public async Task<UserRefreshTokenDto> RefreshAuthTokenAsync(UserRefreshTokenDto token)
        {
            var validationParameters = _jwtBearerOptions.TokenValidationParameters.Clone();
            validationParameters.ValidateLifetime = false; // 此时对于jwt的过期与否已经不关心
            var handler = _jwtBearerOptions.SecurityTokenValidators.OfType<JwtSecurityTokenHandler>().FirstOrDefault()
                ?? new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token.AccessToken, validationParameters, out _);

            var identity = principal.Identities.First();
            var userName = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var refreshTokenId = identity.Claims.FirstOrDefault(c => c.Type == RefreshTokenIdClaimType)?.Value;
            var refreshToken = await _redisProvider.StringGetAsync($"refresh-token:{userName}-{refreshTokenId}");
            if (refreshToken != token.RefreshToken)
            {
                return null;
            }

            await _redisProvider.KeyDelAsync($"refresh-token:{userName}-{refreshTokenId}");
            var user = await _adoQuerierRepository.QueryFirstOrDefaultAsync<User>($"SELECT * FROM [User] WHERE userName = @UserName AND isDeleted = 0", new { UserName = userName });
            if (user == null)
            {
                return null;
            }

            var result = await CreateTokenAsync(user);

            return new UserRefreshTokenDto()
            {
                RefreshToken = result.refreshToken,
                AccessToken = result.accessToken
            };
        }

        public async Task TestAsync()
        {
            _contextAPIDbContext.DbConnection.Execute
                       ("INSERT INTO [User] (userName,password,isDeleted,createBy,createTime,role) VALUES (@UserName, @Password, 0, @CreateBy, @CreateTime, @Role)",
                       new { UserName = "test10", Password = "666666", Role = 1, CreateBy = 1, CreateTime = DateTime.Now });
            throw new Exception();
            _contextAPIDbContext.DbConnection.Execute
               ("INSERT INTO [User] (userName,password,isDeleted,createBy,createTime,role) VALUES (@UserName, @Password, 0, @CreateBy, @CreateTime, @Role)",
               new { UserName = "test11", Password = "666666", Role = 1, CreateBy = 1, CreateTime = DateTime.Now });
        }
    }
}
