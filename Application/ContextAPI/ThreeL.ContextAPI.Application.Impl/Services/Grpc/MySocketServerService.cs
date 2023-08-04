using Grpc.Core;
using ThreeL.ContextAPI.Application.Contract.Protos;
using ThreeL.ContextAPI.Domain.Aggregates.UserAggregate;
using ThreeL.Infra.Repository.IRepositories;

namespace ThreeL.ContextAPI.Application.Impl.Services.Grpc
{
    public class MySocketServerService : SocketServerService.SocketServerServiceBase
    {
        private readonly IAdoQuerierRepository<ContextAPIDbContext> _adoQuerierRepository;
        private readonly IAdoExecuterRepository<ContextAPIDbContext> _adoExecuterRepository;

        public MySocketServerService(IAdoQuerierRepository<ContextAPIDbContext> adoQuerierRepository,
                                     IAdoExecuterRepository<ContextAPIDbContext> adoExecuterRepository)
        {
            _adoQuerierRepository = adoQuerierRepository;
            _adoExecuterRepository = adoExecuterRepository;
        }

        public async override Task<SocketServerUserLoginResponse> SocketServerUserLogin(SocketServerUserLoginRequest request, ServerCallContext context)
        {
            var userName = context.GetHttpContext().User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
                return new SocketServerUserLoginResponse() { Result = false };
            var userid = long.Parse(userName);
            var user = await _adoQuerierRepository
                .QueryFirstOrDefaultAsync<User>("SELECT * FROM [User] WHERE id= @UserId AND isDeleted = 0", new { UserId = userid });

            return new SocketServerUserLoginResponse() { Result = (user != null) };
        }
    }
}
