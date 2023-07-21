using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.ContextAPI.Domain.Aggregates.UserAggregate;
using ThreeL.Infra.Repository.IRepositories;

namespace ThreeL.ContextAPI.Application.Impl.Services
{
    public class UserService : IAppService, IUserService
    {
        private IAdoQuerierRepository<ContextAPIDbContext> _adoQuerierRepository;
        private IAdoExecuterRepository<ContextAPIDbContext> _adoExecuterRepository;

        public UserService(IAdoQuerierRepository<ContextAPIDbContext> adoQuerierRepository,
                           IAdoExecuterRepository<ContextAPIDbContext> adoExecuterRepository)
        {
            _adoQuerierRepository = adoQuerierRepository;
            _adoExecuterRepository = adoExecuterRepository;
        }

        public async Task<User> SearchAllUsersAsync()
        {
            var users = await _adoQuerierRepository.QueryAsync<User>("SELECT * FROM [User]");

            return users.FirstOrDefault();
        }
    }
}
