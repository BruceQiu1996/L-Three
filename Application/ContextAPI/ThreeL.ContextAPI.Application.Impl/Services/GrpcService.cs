using AutoMapper;
using Grpc.Core;
using ThreeL.ContextAPI.Application.Contract.Protos;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.ContextAPI.Domain.Aggregates.File;
using ThreeL.ContextAPI.Domain.Aggregates.UserAggregate;
using ThreeL.ContextAPI.Domain.Aggregates.UserAggregate.Metadata;
using ThreeL.Infra.Core.Metadata;
using ThreeL.Infra.Dapper.Repositories;
using ThreeL.Infra.Redis;
using ThreeL.Shared.Application;

namespace ThreeL.ContextAPI.Application.Impl.Services
{
    public class GrpcService : IGrpcService
    {
        private readonly IMapper _mapper;
        private readonly DapperRepository<ContextAPIDbContext> _dapperRepository;
        private readonly IRedisProvider _redisProvider;

        public GrpcService(DapperRepository<ContextAPIDbContext> dapperRepository, IMapper mapper, IRedisProvider redisProvider)
        {
            _mapper = mapper;
            _redisProvider = redisProvider;
            _dapperRepository = dapperRepository;
        }

        public async Task<SocketServerUserLoginResponse> SocketServerUserLogin(SocketServerUserLoginRequest request, ServerCallContext context)
        {
            var userIdentity = context.GetHttpContext().User.Identity?.Name;
            if (string.IsNullOrEmpty(userIdentity))
                return new SocketServerUserLoginResponse() { Result = false };
            var userid = long.Parse(userIdentity);
            var user = await _dapperRepository
                .QueryFirstOrDefaultAsync<User>("SELECT * FROM [User] WHERE id= @UserId AND isDeleted = 0", new { UserId = userid });

            return new SocketServerUserLoginResponse() { Result = (user != null) };
        }

        public async Task<FileInfoResponse> FetchFileInfo(FileInfoRequest request, ServerCallContext context)
        {
            var userIdentity = context.GetHttpContext().User.Identity?.Name;
            if (string.IsNullOrEmpty(userIdentity))
                return new FileInfoResponse() { Result = false };

            var userid = long.Parse(userIdentity);
            var record =
                await _dapperRepository.QueryFirstOrDefaultAsync<FileRecord>("SELECT TOP 1 * FROM [FILE] WHERE Id = @Id",
                new { request.Id });

            if (record == null || record.CreateBy != userid)
                return new FileInfoResponse() { Result = false };

            return new FileInfoResponse()
            {
                Id = record.Id,
                Name = record.FileName,
                Size = record.Size,
                RemoteLocation = record.Location,
                Result = true
            };
        }

        public async Task<ChatRecordPostResponse> PostChatRecord(IAsyncStreamReader<ChatRecordPostRequest> requestStream, ServerCallContext context)
        {
            List<ChatRecord> requests = new List<ChatRecord>();
            while (await requestStream.MoveNext())
            {
                var record = _mapper.Map<ChatRecord>(requestStream.Current);
                if (record.FileId == 0) record.FileId = null;
                requests.Add(record);
            }

            await _dapperRepository.ExecuteAsync("INSERT INTO ChatRecord([FROM],[TO],MESSAGEID,MESSAGE,MessageRecordType,ImageType,SendTime,FileId)" +
                "VALUES(@From,@To,@MessageId,@Message,@MessageRecordType,@ImageType,@SendTime,@FileId)", requests);

            return new ChatRecordPostResponse() { Result = true };
        }

        public async Task<ChatRecordPostResponse> PostChatRecordSingle(ChatRecordPostRequest request, ServerCallContext context)
        {
            var record = _mapper.Map<ChatRecord>(request);
            if (record.FileId == 0) record.FileId = null;
            await _dapperRepository.ExecuteAsync("INSERT INTO ChatRecord([FROM],[TO],MESSAGEID,MESSAGE,MessageRecordType,ImageType,SendTime,FileId)" +
                "VALUES(@From,@To,@MessageId,@Message,@MessageRecordType,@ImageType,@SendTime,@FileId)", record);

            return new ChatRecordPostResponse() { Result = true };
        }

        public async Task<ChatRecordWithdrawResponse> WithdrawChatRecord(ChatRecordWithdrawRequest request, ServerCallContext context)
        {
            var userIdentity = context.GetHttpContext().User.Identity?.Name;
            var userid = long.Parse(userIdentity);

            var message = await _dapperRepository
                .QueryFirstAsync<ChatRecord>("SELECT Top 1 * FROM ChatRecord WHERE MessageId = @MessageId AND [From] = @From", new
                {
                    request.MessageId,
                    From = userid
                });
            await _dapperRepository
                .ExecuteAsync("UPDATE ChatRecord SET Withdrawed = 1 where Id = @Id", new
                {
                    MessageType = MessageRecordType.Withdraw,
                    message.Id
                });

            return new ChatRecordWithdrawResponse() { Result = true };
        }

        public async Task<AddFriendResponse> AddFriend(AddFriendRequest request, ServerCallContext context)
        {
            var userIdentity = context.GetHttpContext().User.Identity?.Name;
            var userid = long.Parse(userIdentity);
            var users = await _dapperRepository
                .QueryAsync<User>("SELECT * FROM [User] WHERE Id  = @ActiverId or Id = @PassiverId and IsDeleted = 0", new
                {
                    ActiverId = userid,
                    PassiverId = request.FriendId
                });

            if (users == null || users.Count() != 2)
            {
                return new AddFriendResponse() { Result = false, Message = "账号异常" };
            }
            var applys = await _dapperRepository
                .QueryAsync<FriendApply>("SELECT * FROM FriendApply WHERE (Activer = @ActiverId AND Passiver = @PassiverId) OR (Activer = @PassiverId AND Passiver = @ActiverId) AND Status = @Status", new
                {
                    ActiverId = userid,
                    PassiverId = request.FriendId,
                    Status = FriendApplyStatus.TobeProcessed
                });

            if (applys?.Count() > 0)
            {
                return new AddFriendResponse() { Result = false, Message = "已存在未处理的申请好友请求" };
            }

            await _dapperRepository.ExecuteAsync("INSERT INTO FriendApply(Activer,Passiver,CreateTime,Status) VALUES(@Activer,@Passiver,getdate(),@Status)", new
            {
                Activer = userid,
                Passiver = request.FriendId,
                Status = FriendApplyStatus.TobeProcessed
            });

            return new AddFriendResponse() { Result = true };
        }

        public async Task<ReplyAddFriendResponse> ReplyAddFriend(ReplyAddFriendRequest request, ServerCallContext context)
        {
            var userIdentity = context.GetHttpContext().User.Identity?.Name;
            var userid = long.Parse(userIdentity);
            var apply = await _dapperRepository
                .QueryFirstOrDefaultAsync<FriendApply>("SELECT * FROM FriendApply WHERE Id = @Id", new
                {
                    Id = request.RequestId,
                });
            if (apply == null || apply.Status != FriendApplyStatus.TobeProcessed)
            {
                return new ReplyAddFriendResponse() { Result = false, Message = "申请不存在" };
            }

            if (apply.Passiver != userid)
            {
                return new ReplyAddFriendResponse() { Result = false, Message = "无权限" };
            }

            var users = await _dapperRepository
                .QueryAsync<User>("SELECT * FROM [User] WHERE Id  = @ActiverId or Id = @PassiverId and IsDeleted = 0", new
                {
                    ActiverId = apply.Activer,
                    PassiverId = apply.Passiver
                });

            if (users == null || users.Count() != 2)
            {
                return new ReplyAddFriendResponse() { Result = false, Message = "账号异常" };
            }

            if (request.Agree)
            {
                await _dapperRepository.ExecuteAsync("UPDATE FriendApply SET Status = @Status,ProcessTime = getdate() WHERE Id = @Id", new
                {
                    Id = request.RequestId,
                    Status = FriendApplyStatus.Accept
                });
                await _dapperRepository.ExecuteAsync("INSERT INTO Friend(Activer,Passiver,CreateTime) VALUES(@UserId,@FriendId,getdate())", new
                {
                    UserId = apply.Activer,
                    FriendId = apply.Passiver
                });

                await _redisProvider.SetAddAsync(Const.FRIEND_RELATION, new string[] { $"{apply.Activer}-{apply.Passiver}" });
            }
            else
            {
                await _dapperRepository.ExecuteAsync("UPDATE FriendApply SET Status = @Status,ProcessTime = getdate() WHERE Id = @Id", new
                {
                    Id = request.RequestId,
                    Status = FriendApplyStatus.Reject
                });
            }

            var activer = users.First(x => x.Id == apply.Activer);
            var passiver = users.First(x => x.Id == apply.Passiver);

            return new ReplyAddFriendResponse()
            {
                Result = true,
                ActiverId = activer.Id,
                PassiverId = passiver.Id,
                ActiverAvatarId = activer.Avatar == null ? 0 : activer.Avatar.Value,
                PassiverAvatarId = passiver.Avatar == null ? 0 : passiver.Avatar.Value,
                ActiverName = activer.UserName,
                PassiverName = passiver.UserName,
            };
        }
    }
}
