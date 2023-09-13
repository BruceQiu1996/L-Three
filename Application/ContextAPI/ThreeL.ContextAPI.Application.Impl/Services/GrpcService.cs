using Amazon.Runtime.Internal;
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

            return new SocketServerUserLoginResponse() { Result = (user != null), UserName = user.UserName, UserId = user == null ? 0 : user.Id };
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
            await _dapperRepository.ExecuteAsync("INSERT INTO ChatRecord([FROM],[FROMNAME],[TO],MESSAGEID,MESSAGE,MessageRecordType,ImageType,SendTime,FileId,IsGroup)" +
                "VALUES(@From,@FromName,@To,@MessageId,@Message,@MessageRecordType,@ImageType,@SendTime,@FileId,@IsGroup)", record);

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

                await _redisProvider.SetAddAsync(CommonConst.FRIEND_RELATION, new string[] { $"{apply.Activer}-{apply.Passiver}" });
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

        public async Task<InviteFriendsIntoGroupResponse> InviteFriendsIntoGroup(InviteFriendsIntoGroupRequest request, ServerCallContext context)
        {
            var userIdentity = context.GetHttpContext().User.Identity?.Name;
            var userid = long.Parse(userIdentity);

            var group = await _dapperRepository
                .QueryFirstOrDefaultAsync<Group>("SELECT * FROM [Group] WHERE Id = @Id", new
                {
                    Id = request.GroupId
                });

            if (group == null)
            {
                return new InviteFriendsIntoGroupResponse() { Result = false, Message = "群组数据不存在" };
            }

            var members = group.Members.Split(",").Select(long.Parse).ToList();

            if (!members.Contains(userid))
            {
                return new InviteFriendsIntoGroupResponse() { Result = false, Message = "无权限" };
            }

            var ids = request.Friends.Split(",").Select(long.Parse).ToList();
            var result = new List<long>();
            foreach (var id in ids)
            {
                if (!members.Contains(id))
                {
                    members.Add(id);
                    result.Add(id);
                }
            }

            var newMembers = string.Join(",", members);
            await _dapperRepository.ExecuteAsync("UPDATE [Group] SET Members = @Members WHERE Id = @Id", new
            {
                Members = newMembers,
                group.Id
            });

            //更新redis
            await _redisProvider.SetAddAsync(string.Format(CommonConst.GROUP, group.Id), ids.Select(x => x.ToString()).ToArray());

            return new InviteFriendsIntoGroupResponse()
            {
                Result = true,
                Friends = string.Join(",", result),
                GroupId = group.Id,
                GroupName = group.Name,
                AvatarId = group.Avatar == null ? 0 : group.Avatar.Value
            };
        }

        public async Task<ValidateRelationResponse> ValidateRelation(ValidateRelationRequest request, ServerCallContext context)
        {
            var userIdentity = context.GetHttpContext().User.Identity?.Name;
            var userid = long.Parse(userIdentity);
            if (request.IsGroup)
            {
                var relation = await _dapperRepository.QueryFirstOrDefaultAsync<Friend>("SELECT * FROM FRIEND WHERE (ACTIVER = @Id AND PASSIVER = @FID) OR (ACTIVER = @FID AND PASSIVER = @Id)", new
                {
                    Id = userid,
                    FID = request.To
                });

                if (relation == null)
                {
                    return new ValidateRelationResponse() { Result = false };
                }

                await _redisProvider.SetAddAsync(CommonConst.FRIEND_RELATION, new[] { $"{relation.Activer}-{relation.Passiver}" });

                return new ValidateRelationResponse() { Result = true };
            }
            else
            {
                var group = await _dapperRepository.QueryFirstOrDefaultAsync<Group>("SELECT * FROM [Group] WHERE Id = @Id", new
                {
                    Id = request.To
                });

                if (group == null)
                {
                    return new ValidateRelationResponse() { Result = false };
                }

                var contain = group.Members.Split(",").Select(long.Parse).ToList().Contains(userid);
                if (!contain)
                {
                    return new ValidateRelationResponse() { Result = false };
                }
                await _redisProvider.SetAddAsync(string.Format(CommonConst.GROUP, group.Id), new[] { userid.ToString() });

                return new ValidateRelationResponse() { Result = true };
            }
        }

        public async Task<VoiceChatRecordPostResponse> PostVoiceChatRecordSingle(VoiceChatRecordPostRequest request, ServerCallContext context)
        {
            var userIdentity = context.GetHttpContext().User.Identity?.Name;
            var userid = long.Parse(userIdentity);
            var record = _mapper.Map<VoiceChatRecord>(request);
            record.From = userid;
            record.Status = VoiceChatStatus.Initialized;

            await _dapperRepository.ExecuteAsync("INSERT INTO VoiceChatRecord([ChatKey],[SendTime],[From],[TO],FromPlatform,Status)" +
                "VALUES(@ChatKey,@SendTime,@From,@To,@FromPlatform,@Status)", record);

            return new VoiceChatRecordPostResponse() { Result = true };
        }

        public async Task<VoiceChatRecorStatusResponse> GetVoiceChatStatus(VoiceChatRecorStatusRequest request, ServerCallContext context)
        {
            var record = await _dapperRepository
                .QueryFirstOrDefaultAsync<VoiceChatRecord>("SELECT Top 1 * FROM VoiceChatRecord WHERE ChatKey = @key", new { key = request.ChatKey });

            return new VoiceChatRecorStatusResponse()
            {
                Started = record.Status != VoiceChatStatus.Initialized
            };
        }

        public async Task<VoiceChatRecorStatusUpdateResponse> UpdateVoiceChatStatus(VoiceChatRecorStatusUpdateRequest request, ServerCallContext context)
        {
            var status = (VoiceChatStatus)request.Status;
            if (status == VoiceChatStatus.NotAccept || status == VoiceChatStatus.Rejected || status == VoiceChatStatus.Canceled)
            {
                await _dapperRepository
                    .ExecuteAsync("UPDATE VoiceChatRecord SET Status = @Status WHERE ChatKey = @key", new { key = request.ChatKey, request.Status });
                //新增通话结束的聊天记录
                var record = await _dapperRepository
                        .QueryFirstOrDefaultAsync<VoiceChatRecord>("SELECT Top 1 * FROM VoiceChatRecord WHERE ChatKey = @key", new { key = request.ChatKey });
                var temp = new ChatRecord()
                {
                    From = record.From,
                    To = record.To,
                    FromName = request.FromName,
                    Message = status.ToString(),
                    MessageId = record.ChatKey,
                    SendTime = request.SendTime.ToDateTime().ToLocalTime(),
                    MessageRecordType = MessageRecordType.VoiceChat
                };
                await _dapperRepository.ExecuteAsync("INSERT INTO ChatRecord([FROM],[FROMNAME],[TO],MESSAGEID,MESSAGE,MessageRecordType,SendTime,IsGroup)" +
                    "VALUES(@From,@FromName,@To,@MessageId,@Message,@MessageRecordType,@SendTime,0)", temp);
            }
            else if (status == VoiceChatStatus.Started)
            {
                await _dapperRepository
                    .ExecuteAsync("UPDATE VoiceChatRecord SET Status = @Status,StartTime = @StartTime WHERE ChatKey = @key", new { key = request.ChatKey, request.Status, StartTime = DateTime.Now });
            }
            else if (status == VoiceChatStatus.Finished)
            {
                await _dapperRepository
                    .ExecuteAsync("UPDATE VoiceChatRecord SET Status = @Status,EndTime = @EndTime WHERE ChatKey = @key", new { key = request.ChatKey, request.Status, EndTime = DateTime.Now });
            }

            return new VoiceChatRecorStatusUpdateResponse()
            {
                Result = true
            };
        }
    }
}
