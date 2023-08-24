using AutoMapper;
using ThreeL.ContextAPI.Application.Contract.Configurations;
using ThreeL.ContextAPI.Application.Contract.Dtos.Relation;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.Infra.Redis;
using ThreeL.Infra.Repository.IRepositories;
using ThreeL.Shared.Application.Contract.Services;

namespace ThreeL.ContextAPI.Application.Impl.Services
{
    public class FriendService : IFriendService, IPreheatService
    {
        private readonly IAdoQuerierRepository<ContextAPIDbContext> _adoQuerierRepository;
        private readonly IAdoExecuterRepository<ContextAPIDbContext> _adoExecuterRepository;
        private readonly IRedisProvider _redisProvider;
        private readonly IMapper _mapper;

        public FriendService(IAdoQuerierRepository<ContextAPIDbContext> adoQuerierRepository,
                             IRedisProvider redisProvider,
                             IAdoExecuterRepository<ContextAPIDbContext> adoExecuterRepository,
                             IMapper mapper)
        {
            _redisProvider = redisProvider;
            _adoQuerierRepository = adoQuerierRepository;
            _adoExecuterRepository = adoExecuterRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FriendDto>> GetFriendsAsync(long userId)
        {
            var friends = await _adoQuerierRepository
                .QueryAsync<FriendDto>(
                "SELECT u1.id AS ActiverId,u1.userName AS ActiverName,u2.id AS PassiverId,u2.userName AS PassiverName,Friend.ActiverRemark,Friend.PassiverRemark " +
                "FROM Friend INNER JOIN [USER] u1 ON u1.id = Friend.Activer INNER JOIN [USER] u2 ON u2.id = Friend.Passiver" +
                " WHERE (Friend.Activer = @Id OR Friend.Passiver = @Id) AND u1.isDeleted = 0 AND u2.isDeleted = 0",
                new { Id = userId });

            return friends;
        }

        public async Task<bool> IsFriendAsync(long userId, long fUserId)
        {
            var relation = await _adoQuerierRepository
                .QueryFirstOrDefaultAsync<FriendDto>(
                "SELECT * FROM Friend WHERE (Friend.Activer = @Id AND Friend.Passiver = @FId) OR (Friend.Activer = @FId AND Friend.Passiver = @Id)",
                new { Id = userId, FId = fUserId });

            return relation != null;
        }

        public async Task PreheatAsync()
        {
            var friends = await _adoQuerierRepository
                .QueryAsync<FriendDto>(
                "SELECT u1.id AS ActiverId,u1.userName AS ActiverName,u2.id AS PassiverId,u2.userName AS PassiverName,Friend.ActiverRemark,Friend.PassiverRemark " +
                "FROM Friend INNER JOIN [USER] u1 ON u1.id = Friend.Activer INNER JOIN [USER] u2 ON u2.id = Friend.Passiver" +
                " WHERE u1.isDeleted = 0 AND u2.isDeleted = 0");

            var ids = friends?.Select(x => $"{x.ActiverId}-{x.PassiverId}");
            await _redisProvider.SetAddAsync(Const.FRIEND_RELATIONS, ids == null ? new string[] { } : ids.ToArray());
        }

        public async Task<FriendChatRecordResponseDto> FetchChatRecordsWithFriendAsync(long userId, long friendId, DateTime dateTime)
        {
            var records = await _adoQuerierRepository
                .QueryAsync<ChatRecordResponseDto>("SELECT MessageId,Message,MessageRecordType,ImageType,Withdrawed,SendTime,[From],[To],FileId,f.FileName,f.Size FROM (SELECT TOP 30 * FROM ChatRecord " +
                "WHERE ChatRecord.SendTime < @Time AND ([FROM] = @UserId AND [To] = @FriendId) OR ([FROM] = @FriendId AND [To] = @UserId) ORDER BY SendTime DESC) t LEFT JOIN [File] f ON t.FileId = f.id",
                new
                {
                    UserId = userId,
                    FriendId = friendId,
                    Time = dateTime
                });

            return new FriendChatRecordResponseDto
            {
                FriendId = friendId,
                Records = records?.Select(x => x.ClearDataByWithdrawed())
            };
        }
    }
}
