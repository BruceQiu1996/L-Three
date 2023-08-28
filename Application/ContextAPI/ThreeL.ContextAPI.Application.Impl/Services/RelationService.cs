using AutoMapper;
using ThreeL.ContextAPI.Application.Contract.Dtos.Relation;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.ContextAPI.Domain.Aggregates.UserAggregate;
using ThreeL.Infra.Redis;
using ThreeL.Infra.Repository.IRepositories;
using ThreeL.Shared.Application;
using ThreeL.Shared.Application.Contract.Services;

namespace ThreeL.ContextAPI.Application.Impl.Services
{
    public class RelationService : IRelationService, IPreheatService
    {
        private readonly IAdoQuerierRepository<ContextAPIDbContext> _adoQuerierRepository;
        private readonly IAdoExecuterRepository<ContextAPIDbContext> _adoExecuterRepository;
        private readonly IRedisProvider _redisProvider;
        private readonly IMapper _mapper;

        public RelationService(IAdoQuerierRepository<ContextAPIDbContext> adoQuerierRepository,
                             IRedisProvider redisProvider,
                             IAdoExecuterRepository<ContextAPIDbContext> adoExecuterRepository,
                             IMapper mapper)
        {
            _redisProvider = redisProvider;
            _adoQuerierRepository = adoQuerierRepository;
            _adoExecuterRepository = adoExecuterRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RelationDto>> GetRelationsAndChatRecordsAsync(long userId, DateTime dateTime)
        {
            List<RelationDto> relations = new List<RelationDto>();
            var friends = await _adoQuerierRepository.QueryAsync<FriendDto>(
                "SELECT u1.id AS ActiverId,u1.userName AS ActiverName,u2.id AS PassiverId,u2.userName AS PassiverName,Friend.ActiverRemark,Friend.PassiverRemark,u1.Avatar AS ActiverAvatar,u2.Avatar AS PassiverAvatar,Friend.CreateTime " +
                "FROM Friend INNER JOIN [USER] u1 ON u1.id = Friend.Activer INNER JOIN [USER] u2 ON u2.id = Friend.Passiver" +
                " WHERE (Friend.Activer = @Id OR Friend.Passiver = @Id) AND u1.isDeleted = 0 AND u2.isDeleted = 0",
                new { Id = userId });

            if (friends != null)
            {
                foreach (var friend in friends)
                {
                    //查询聊天记录
                    var records = await _adoQuerierRepository.QueryAsync<ChatRecordResponseDto>("SELECT MessageId,Message,MessageRecordType,ImageType,Withdrawed,SendTime,[From],[To],[InnerId],FileId,f.FileName,f.Size FROM (SELECT TOP 30 * FROM ChatRecord " +
                        "WHERE ChatRecord.SendTime < @Time AND ([FROM] = @UserId AND [To] = @FriendId) OR ([FROM] = @FriendId AND [To] = @UserId) ORDER BY SendTime DESC) t LEFT JOIN [File] f ON t.FileId = f.id",
                    new
                    {
                        UserId = userId,
                        FriendId = friend.GetFriendId(userId),
                        Time = dateTime
                    });

                    relations.Add(new RelationDto()
                    {
                        IsGroup = false,
                        CreateTime = friend.CreateTime,
                        Name = friend.GetFriendName(userId),
                        Remark = friend.GetFriendRemark(userId),
                        Avatar = friend.GetFriendAvatar(userId),
                        Id = friend.GetFriendId(userId),
                        ChatRecords = records?.Select(x => x.ClearDataByWithdrawed())?.ToList()
                    });
                }
            }

            //查询群组
            var groups = await _adoQuerierRepository
                .QueryAsync<Group>("SELECT * FROM [GROUP] WHERE CHARINDEX(@Id, Members) > 0",
                new { Id = userId.ToString() });

            if (groups != null)
            {
                foreach (var group in groups)
                {
                    //查询聊天记录
                    var records = await _adoQuerierRepository.QueryAsync<ChatRecordResponseDto>("SELECT MessageId,Message,MessageRecordType,ImageType,Withdrawed,SendTime,[From],[To],[InnerId],FileId,f.FileName,f.Size FROM (SELECT TOP 30 * FROM GroupChatRecord " +
                        "WHERE GroupChatRecord.SendTime < @Time AND [To] = @GroupId ORDER BY SendTime DESC) t LEFT JOIN [File] f ON t.FileId = f.id",
                    new
                    {
                        GroupId = group.Id,
                        Time = dateTime
                    });


                    relations.Add(new RelationDto()
                    {
                        IsGroup = true,
                        CreateTime = group.CreateTime,
                        Name = group.Name,
                        Remark = null,
                        MemberCount = group.Members?.Split(",")?.Count(),
                        Avatar = group.Avatar,
                        Id = group.Id,
                        ChatRecords = records?.Select(x => x.ClearDataByWithdrawed())?.ToList()
                    });
                }
            }

            return relations;
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
            await _redisProvider.SetAddAsync(CommonConst.FRIEND_RELATION, ids == null ? new string[] { } : ids.ToArray());
        }

        //public async Task<FriendChatRecordResponseDto> FetchChatRecordsWithFriendAsync(long userId, long friendId, DateTime dateTime)
        //{
        //    var records = await _adoQuerierRepository
        //        .QueryAsync<ChatRecordResponseDto>("SELECT MessageId,Message,MessageRecordType,ImageType,Withdrawed,SendTime,[From],[To],FileId,f.FileName,f.Size FROM (SELECT TOP 30 * FROM ChatRecord " +
        //        "WHERE ChatRecord.SendTime < @Time AND ([FROM] = @UserId AND [To] = @FriendId) OR ([FROM] = @FriendId AND [To] = @UserId) ORDER BY SendTime DESC) t LEFT JOIN [File] f ON t.FileId = f.id",
        //        new
        //        {
        //            UserId = userId,
        //            FriendId = friendId,
        //            Time = dateTime
        //        });

        //    return new FriendChatRecordResponseDto
        //    {
        //        FriendId = friendId,
        //        Records = records?.Select(x => x.ClearDataByWithdrawed())
        //    };
        //}

        public async Task<IEnumerable<FriendApplyResponseDto>> FetchAllFriendApplysAsync(long userId)
        {
            var applys = await _adoQuerierRepository
                .QueryAsync<FriendApplyResponseDto>("SELECT FriendApply.Id AS Id, u1.Id AS ActiverId,u1.userName AS ActiverName,u2.Id AS PassiverId,u2.userName AS PassiverName ,FriendApply.CreateTime,FriendApply.ProcessTime, FriendApply.Status FROM FriendApply INNER JOIN [User] u1 ON u1.Id = FriendApply.Activer INNER JOIN [User] u2 ON u2.Id = FriendApply.Passiver WHERE FriendApply.Activer = @Id OR FriendApply.Passiver = @Id", new
                {
                    Id = userId
                });

            return applys == null ? new List<FriendApplyResponseDto>() : applys;
        }
    }
}
