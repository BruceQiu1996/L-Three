using Amazon.Runtime.Internal;
using AutoMapper;
using Grpc.Core;
using ThreeL.ContextAPI.Application.Contract.Protos;
using ThreeL.ContextAPI.Domain.Aggregates.File;
using ThreeL.ContextAPI.Domain.Aggregates.UserAggregate;
using ThreeL.Infra.Core.File;
using ThreeL.Infra.Repository.IRepositories;

namespace ThreeL.ContextAPI.Application.Impl.Services.Grpc
{
    public class MySocketServerService : SocketServerService.SocketServerServiceBase
    {
        private readonly IMapper _mapper;
        private readonly IAdoQuerierRepository<ContextAPIDbContext> _adoQuerierRepository;
        private readonly IAdoExecuterRepository<ContextAPIDbContext> _adoExecuterRepository;

        public MySocketServerService(IAdoQuerierRepository<ContextAPIDbContext> adoQuerierRepository,
                                     IAdoExecuterRepository<ContextAPIDbContext> adoExecuterRepository, IMapper mapper)
        {
            _mapper = mapper;
            _adoQuerierRepository = adoQuerierRepository;
            _adoExecuterRepository = adoExecuterRepository;
        }

        public async override Task<SocketServerUserLoginResponse> SocketServerUserLogin(SocketServerUserLoginRequest request, ServerCallContext context)
        {
            var userIdentity = context.GetHttpContext().User.Identity?.Name;
            if (string.IsNullOrEmpty(userIdentity))
                return new SocketServerUserLoginResponse() { Result = false };
            var userid = long.Parse(userIdentity);
            var user = await _adoQuerierRepository
                .QueryFirstOrDefaultAsync<User>("SELECT * FROM [User] WHERE id= @UserId AND isDeleted = 0", new { UserId = userid });

            return new SocketServerUserLoginResponse() { Result = (user != null) };
        }

        public async override Task<FileInfoResponse> FetchFileInfo(FileInfoRequest request, ServerCallContext context)
        {
            var userIdentity = context.GetHttpContext().User.Identity?.Name;
            if (string.IsNullOrEmpty(userIdentity))
                return new FileInfoResponse() { Result = false };

            var userid = long.Parse(userIdentity);
            var record =
                await _adoQuerierRepository.QueryFirstOrDefaultAsync<FileRecord>("SELECT TOP 1 * FROM [FILE] WHERE Id = @Id",
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

        public async override Task<ChatRecordPostResponse> PostChatRecord(IAsyncStreamReader<ChatRecordPostRequest> requestStream, ServerCallContext context)
        {
            List<ChatRecord> requests = new List<ChatRecord>();
            while (await requestStream.MoveNext())
            {
                var record = _mapper.Map<ChatRecord>(requestStream.Current);
                if (record.FileId == 0) record.FileId = null;
                requests.Add(record);
            }

            await _adoExecuterRepository.ExecuteAsync("INSERT INTO ChatRecord([FROM],[TO],MESSAGEID,MESSAGE,MessageRecordType,ImageType,SendTime,FileId)" +
                "VALUES(@From,@To,@MessageId,@Message,@MessageRecordType,@ImageType,@SendTime,@FileId)", requests);

            return new ChatRecordPostResponse();
        }

        public async override Task<ChatRecordPostResponse> PostChatRecordSingle(ChatRecordPostRequest request, ServerCallContext context)
        {
            var record = _mapper.Map<ChatRecord>(request);
            if (record.FileId == 0) record.FileId = null;
            await _adoExecuterRepository.ExecuteAsync("INSERT INTO ChatRecord([FROM],[TO],MESSAGEID,MESSAGE,MessageRecordType,ImageType,SendTime,FileId)" +
                "VALUES(@From,@To,@MessageId,@Message,@MessageRecordType,@ImageType,@SendTime,@FileId)", record);

            return new ChatRecordPostResponse();
        }
    }
}
