using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ThreeL.ContextAPI.Application.Contract.Configurations;
using ThreeL.ContextAPI.Application.Contract.Dtos.User;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.ContextAPI.Domain.Aggregates.UserAggregate;
using ThreeL.ContextAPI.Domain.Entities;
using ThreeL.Infra.Core.Cryptography;
using ThreeL.Infra.Core.Enum;
using ThreeL.Infra.Redis;
using ThreeL.Infra.Repository.IRepositories;
using ThreeL.Shared.Application;
using ThreeL.Shared.Application.Contract.Configurations;
using ThreeL.Shared.Application.Contract.Helpers;
using ThreeL.Shared.Application.Contract.Services;
using ThreeL.Shared.Domain.Metadata;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace ThreeL.ContextAPI.Application.Impl.Services
{
    public class UserService : IUserService
    {
        private const string RefreshTokenIdClaimType = "refresh_token_id";
        private readonly IAdoQuerierRepository<ContextAPIDbContext> _adoQuerierRepository;
        private readonly IAdoExecuterRepository<ContextAPIDbContext> _adoExecuterRepository;
        private readonly IRedisProvider _redisProvider;
        private readonly PasswordHelper _passwordHelper;
        private readonly JwtOptions _jwtOptions;
        private readonly JwtBearerOptions _jwtBearerOptions;
        private readonly SystemOptions _systemOptions;
        private readonly FileStorageOptions _storageOptions;
        private readonly IMapper _mapper;

        public UserService(IAdoQuerierRepository<ContextAPIDbContext> adoQuerierRepository,
                           IAdoExecuterRepository<ContextAPIDbContext> adoExecuterRepository,
                           PasswordHelper passwordHelper,
                           IRedisProvider redisProvider,
                           IOptions<JwtOptions> jwtOptions,
                           IOptionsSnapshot<JwtBearerOptions> jwtBearerOptions,
                           IOptions<FileStorageOptions> storageOptions,
                           IOptions<SystemOptions> systemOptions,
                           IMapper mapper)
        {
            _mapper = mapper;
            _passwordHelper = passwordHelper;
            _redisProvider = redisProvider;
            _systemOptions = systemOptions.Value;
            _jwtOptions = jwtOptions.Value;
            _storageOptions = storageOptions.Value;
            _jwtBearerOptions = jwtBearerOptions.Get(JwtBearerDefaults.AuthenticationScheme);
            _adoQuerierRepository = adoQuerierRepository;
            _adoExecuterRepository = adoExecuterRepository;
        }

        public async Task<ServiceResult> CreateUserAsync(UserCreationDto creationDto, long creator)
        {
            var exist = await _adoQuerierRepository
                .QueryFirstOrDefaultAsync<string>("SELECT USERNAME FROM [USER] WHERE USERNAME = @UserName", new { creationDto.UserName });
            if (exist != null)
            {
                return new ServiceResult(HttpStatusCode.BadRequest, "用户名重复");
            }
            creationDto.Password = _passwordHelper.HashPassword(creationDto.Password);
            var user = _mapper.Map<User>(creationDto);
            user.CreateTime = DateTime.Now;
            user.CreateBy = creator;
            await _adoExecuterRepository.ExecuteScalarAsync<User>
                ("INSERT INTO [User] (userName,password,isDeleted,createBy,createTime,role) VALUES (@UserName, @Password, 0, @CreateBy, @CreateTime, @Role)",
                new { creationDto.UserName, creationDto.Password, creationDto.Role, user.CreateBy, user.CreateTime });

            return new ServiceResult(); ;
        }

        public async Task<UserLoginResponseDto> LoginAsync(UserLoginDto userLoginDto)
        {
            var user = await _adoQuerierRepository.QueryFirstOrDefaultAsync<User>("SELECT * FROM [User] WHERE userName = @UserName AND isDeleted = 0",
                new { userLoginDto.UserName }); ;

            if (user == null)
                return default;

            if (!_passwordHelper.VerifyHashedPassword(userLoginDto.Password, user.Password))
            {
                return default;
            }
            else
            {
                await _adoExecuterRepository.ExecuteAsync($"update [user] set lastLoginTime = @Time where id=@Id",
                    new
                    {
                        Time = DateTime.Now,
                        user.Id
                    });

                byte[] avatarBytes = null;
                var token = await CreateTokenAsync(user, userLoginDto.Origin);
                var avatar = await _adoQuerierRepository.QueryFirstOrDefaultAsync<UserAvatar>("SELECT * FROM [UserAvatar] WHERE Id = @Id", new { Id = user.Avatar });
                if (avatar != null && File.Exists(avatar.Location))
                {
                    avatarBytes = File.ReadAllBytes(avatar.Location);
                }
                return new UserLoginResponseDto()
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Role = user.Role.ToString(),
                    AccessToken = token.accessToken,
                    RefreshToken = token.refreshToken,
                    Avatar = avatarBytes,
                    AvatarId = user.Avatar
                };
            }
        }

        public async Task<bool> LoginByCodeAsync(UserLoginDto userLoginDto)
        {
            var user = await _adoQuerierRepository.QueryFirstOrDefaultAsync<User>("SELECT * FROM [User] WHERE userName = @UserName AND isDeleted = 0",
                new { userLoginDto.UserName }); ;

            if (user == null)
                return false;

            await _adoExecuterRepository.ExecuteAsync($"update [user] set lastLoginTime = @Time where id=@Id", new
            {
                Time = DateTime.Now,
                user.Id
            });

            return true;
        }

        private async Task<(string accessToken, string refreshToken)> CreateTokenAsync(User user, string origin)
        {
            var settings = await _redisProvider.HGetAllAsync<JwtSetting>(Const.REDIS_JWT_SECRET_KEY);
            var setting = settings
                .FirstOrDefault(x => x.Key.StartsWith(_systemOptions.Name)
                && x.Value.Issuer == _jwtOptions.Issuer).Value;

            var refreshToken = await CreateRefreshTokenAsync(user.Id);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(RefreshTokenIdClaimType,refreshToken.refreshTokenId)
            };

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(setting.SecretKey));
            var algorithm = SecurityAlgorithms.HmacSha256;
            var signingCredentials = new SigningCredentials(secretKey, algorithm);
            var jwtSecurityToken = new JwtSecurityToken(
                _jwtOptions.Issuer,             //Issuer
                origin,                         //Audience TODO 客户端携带客户端类型头
                claims,
                null,
                DateTime.Now.AddSeconds(_jwtOptions.TokenExpireSeconds),    //expires
                signingCredentials               //Credentials
            );

            return (new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken), refreshToken.refreshToken);
        }

        private async Task<(string refreshTokenId, string refreshToken)> CreateRefreshTokenAsync(long userId)
        {
            var tokenId = Guid.NewGuid().ToString("N");
            var rnBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(rnBytes);
            var token = Convert.ToBase64String(rnBytes);
            await _redisProvider.StringSetAsync($"refresh-token:{userId}-{tokenId}", token, TimeSpan.FromSeconds(_jwtOptions.RefreshTokenExpireSeconds));

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
            var userId = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var refreshTokenId = identity.Claims.FirstOrDefault(c => c.Type == RefreshTokenIdClaimType)?.Value;
            var refreshToken = await _redisProvider.StringGetAsync($"refresh-token:{userId}-{refreshTokenId}");
            if (refreshToken != token.RefreshToken)
            {
                return null;
            }

            await _redisProvider.KeyDelAsync($"refresh-token:{userId}-{refreshTokenId}");
            var user = await _adoQuerierRepository.QueryFirstOrDefaultAsync<User>($"SELECT * FROM [User] WHERE Id = @UserId AND isDeleted = 0", new { UserId = userId });
            if (user == null)
            {
                return null;
            }

            var result = await CreateTokenAsync(user, token.Origin);

            return new UserRefreshTokenDto()
            {
                RefreshToken = result.refreshToken,
                AccessToken = result.accessToken
            };
        }

        /// <summary>
        /// 根据关键字查找用户
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns>查询到的用户</returns>
        public async Task<ServiceResult<IEnumerable<UserRoughlyDto>>> FindUserByKeyword(long userId, string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return new ServiceResult<IEnumerable<UserRoughlyDto>>(HttpStatusCode.BadRequest, "错误的请求");
            }
            //查找除了自己的用户，附带查询是否是好友
            var user = await _adoQuerierRepository
                .QueryAsync<dynamic>($"SELECT t1.id AS Id,UserName, Role,Avatar,Sign,t2.id AS Fid FROM " +
                $"(SELECT * FROM [User] WHERE userName like @Keyword AND isDeleted = 0 AND id !=@UserId) t1 LEFT JOIN " +
                $"(SELECT * FROM FRIEND WHERE Activer = @UserId OR Passiver = @UserId) t2 ON t1.id = t2.Activer OR t1.id = t2.Passiver", new
                {
                    Keyword = $"%{keyword}%",
                    UserId = userId
                });

            if (user == null)
            {
                return new ServiceResult<IEnumerable<UserRoughlyDto>>(new List<UserRoughlyDto>());
            }
            else
            {
                return new ServiceResult<IEnumerable<UserRoughlyDto>>(user.Select(x => new UserRoughlyDto()
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Role = ((Role)x.Role).GetDescription(),
                    Avatar = x.Avatar,
                    Sign = x.Sign,
                    IsFriend = x.Fid != 0 && x.Fid != null
                }));
            }
        }

        public async Task<ServiceResult<UserAvatarCheckExistResponseDto>> CheckAvatarExistInServerAsync(string code, long userId)
        {
            var record =
                await _adoQuerierRepository.QueryFirstOrDefaultAsync<UserAvatar>("SELECT TOP 1 * FROM [UserAvatar] WHERE CODE = @Code AND CREATEBY = @UserId",
                new { Code = code, UserId = userId });

            if (record != null && File.Exists(record.Location)) //云存储则需要用其他判断文件存在的方式,一般是接口调用
            {
                //滑动更新头像文件的创建时间 TODO采用发布的方式
                await _adoExecuterRepository.ExecuteAsync("UPDATE [UserAvatar] SET CreateTime = GETDATE() WHERE Id = @Id", new { record.Id });
                await _adoExecuterRepository.ExecuteAsync("UPDATE [User] SET Avatar = @Avatar WHERE Id = @Id", new { record.Id, Avatar = record.Id });
                return new ServiceResult<UserAvatarCheckExistResponseDto>(new UserAvatarCheckExistResponseDto()
                {
                    Exist = true,
                    Avatar = File.ReadAllBytes(record.Location)
                });
            }

            return new ServiceResult<UserAvatarCheckExistResponseDto>(new UserAvatarCheckExistResponseDto()
            {
                Exist = false,
            });
        }

        public async Task<ServiceResult<FileInfo>> UploadUserAvatarAsync(long userId, string code, IFormFile file)
        {
            if (file.Length > _storageOptions.AvatarMaxSize)
            {
                return new ServiceResult<FileInfo>(HttpStatusCode.BadRequest, "图片大小不符要求");
            }

            var sha256code = file.OpenReadStream().ToSHA256();
            if (sha256code != code)
            {
                return new ServiceResult<FileInfo>(HttpStatusCode.BadRequest, "图片损坏");
            }

            var fileExtension = Path.GetExtension(file.FileName);
            var savepath = Path.Combine(_storageOptions.StorageLocation, "avatars", DateTime.Now.ToString("yyyy-MM"), $"user-{userId}");
            var fileName = $"{Path.GetRandomFileName()}{fileExtension}";
            var fullName = Path.Combine(savepath, fileName);

            if (!Directory.Exists(savepath))
            {
                Directory.CreateDirectory(savepath);
            }

            using (var fs = File.Create(fullName))
            {
                await file.CopyToAsync(fs);
                await fs.FlushAsync();
            }

            var sql = "INSERT INTO [UserAvatar](CreateBy,FileName,Code,Location,createTime) VALUES(@CreateBy,@FileName,@Code,@Location,GETDATE());SELECT CAST(SCOPE_IDENTITY() as bigint)";
            var fileId = await _adoQuerierRepository.QueryFirstAsync<long>(sql, new
            {
                CreateBy = userId,
                file.FileName,
                Code = code,
                Location = fullName
            });

            await _adoExecuterRepository.ExecuteAsync("UPDATE [User] SET Avatar = @Avatar WHERE Id = @Id", new
            {
                Id = userId,
                Avatar = fileId
            });

            return new ServiceResult<FileInfo>(new FileInfo(fullName));
        }

        public async Task<ServiceResult<FileInfo>> DownloadUserAvatarAsync(long avatarId, long userId)
        {
            var avatar = await _adoQuerierRepository.QueryFirstOrDefaultAsync<string>("SELECT LOCATION FROM UserAvatar WHERE Id = @Id and CREATEBY = @UserId",
                new
                {
                    Id = avatarId,
                    UserId = userId
                });

            if (avatar == null || !File.Exists(avatar))
            {
                return new ServiceResult<FileInfo>(HttpStatusCode.BadRequest, "头像数据异常");
            }

            return new ServiceResult<FileInfo>(new FileInfo(avatar));
        }

        public async Task<ServiceResult<GroupCreationResponseDto>> CreateGroupChatAsync(long userId, string groupName)
        {
            var user = await _adoQuerierRepository.QueryFirstOrDefaultAsync<User>("SELECT * FROM [User] WHERE Id = @UserId AND isDeleted = 0",
                new { UserId = userId });

            if (user == null)
                return new ServiceResult<GroupCreationResponseDto>(HttpStatusCode.BadRequest, "用户数据异常");

            var group = await _adoQuerierRepository.QueryFirstAsync<Group>("INSERT INTO [GROUP] (NAME,CREATETIME,CREATEBY,MEMBERS) VALUES(@Name,getdate(),@UserId,@Members);SELECT * FROM [GROUP] WHERE id = CAST(SCOPE_IDENTITY() as Int)",
                new
                {
                    Name = groupName,
                    UserId = userId,
                    Members = userId.ToString()
                });

            await _redisProvider.SetAddAsync(string.Format(CommonConst.GROUP, group.Id), new[] { userId.ToString() });

            var resp = _mapper.Map<GroupCreationResponseDto>(group);
            resp.Users = new List<UserRoughlyDto>() { new UserRoughlyDto()
            {
                Id = user.Id,
                UserName = user.UserName,
                Role = user.Role.GetDescription(),
                Avatar = user.Avatar,
                Sign = user.Sign,
                IsFriend = true
            } };

            return new ServiceResult<GroupCreationResponseDto>(resp);
        }

        public async Task<ServiceResult> InviteUserJoinGroupChatAsync(long userId, long groupId, IEnumerable<long> ids)
        {
            var group = await _adoQuerierRepository.QueryFirstOrDefaultAsync<Group>("SELECT * FROM [GROUP] WHERE Id = @Id",
                               new { Id = groupId });
            if (group == null)
            {
                return new ServiceResult(HttpStatusCode.BadRequest, "群组数据异常");
            }
            //查询用户的所有好友信息
            var friends = await _adoQuerierRepository.QueryAsync<Friend>("SELECT * FROM [FRIEND] WHERE ACTIVER = @UserId OR PASSIVER = @UserId",
                               new { UserId = userId });

            if (friends == null || friends.Count() <= 0)
                return new ServiceResult(HttpStatusCode.BadRequest, "用户数据异常");

            bool error = false;
            foreach (var friend in ids)
            {
                if (friends.FirstOrDefault(x => x.Activer == userId && x.Passiver == friend) == null
                    && friends.FirstOrDefault(x => x.Activer == friend && x.Passiver == userId) == null)
                {
                    error = true;
                    break;
                }
            }

            if (error)
            {
                return new ServiceResult(HttpStatusCode.BadRequest, "用户数据异常");
            }

            var tempGroup = await _adoQuerierRepository.QueryFirstOrDefaultAsync<Group>("SELECT * FROM [GROUP] WHERE Id = @Id",
                              new { Id = groupId });
            var mbs = $"{tempGroup.Members},{string.Join(",", ids)}";
            await _adoExecuterRepository.ExecuteAsync("UPDATE [GROUP] SET Members = @Members", new { Members = mbs });
            await _redisProvider.SetAddAsync(string.Format(CommonConst.GROUP, groupId), ids.Select(x => x.ToString()).ToArray());

            return new ServiceResult();
        }

        public async Task<ServiceResult<UserRoughlyDto>> FetchUserInfoByIdAsync(long userId, long sUserId)
        {
            var user = await _adoQuerierRepository.QueryFirstOrDefaultAsync<User>($"SELECT Top 1 * FROM [User] WHERE isDeleted = 0 AND id = @SUserId", new
            {
                SUserId = sUserId
            });


            if (user == null)
            {
                return new ServiceResult<UserRoughlyDto>(HttpStatusCode.BadRequest, "数据异常");
            }

            var friend = await _adoQuerierRepository.QueryFirstOrDefaultAsync<Friend>($"SELECT Top 1 * FROM FRIEND WHERE (Activer = @UserId AND Passiver = @SUserId) OR (Passiver = @UserId AND Activer = @SUserId)", new
            {
                SUserId = sUserId,
                UserId = userId
            });

            return new ServiceResult<UserRoughlyDto>(new UserRoughlyDto()
            {
                Id = user.Id,
                UserName = user.UserName,
                Role = user.Role.GetDescription(),
                CreateTime = user.CreateTime,
                Avatar = user.Avatar,
                Sign = user.Sign,
                IsFriend = friend != null,
                RemarkName = friend?.GetFriendRemarkName(sUserId),
                FriendCreateTime = friend?.CreateTime
            });
        }
    }
}
