﻿using AutoMapper;
using ThreeL.ContextAPI.Application.Contract.Dtos.Relation;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.Infra.Repository.IRepositories;

namespace ThreeL.ContextAPI.Application.Impl.Services
{
    public class FriendService : IFriendService, IAppService
    {
        private readonly IAdoQuerierRepository<ContextAPIDbContext> _adoQuerierRepository;
        private readonly IAdoExecuterRepository<ContextAPIDbContext> _adoExecuterRepository;
        private readonly IMapper _mapper;

        public FriendService(IAdoQuerierRepository<ContextAPIDbContext> adoQuerierRepository,
                             IAdoExecuterRepository<ContextAPIDbContext> adoExecuterRepository,
                             IMapper mapper)
        {
            _adoQuerierRepository = adoQuerierRepository;
            _adoExecuterRepository = adoExecuterRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FriendDto>> GetFriendsAsync(long userId)
        {
            var frieds = await _adoQuerierRepository
                .QueryAsync<FriendDto>(
                "SELECT u1.id AS ActiverId,u1.userName AS ActiverName,u2.id AS PassiverId,u2.userName AS PassiverName,Friend.ActiverRemark,Friend.PassiverRemark " +
                "FROM Friend INNER JOIN [USER] u1 ON u1.id = Friend.Activer INNER JOIN [USER] u2 ON u2.id = Friend.Passiver" +
                " WHERE (Friend.Activer = @Id OR Friend.Passiver = @Id) AND u1.isDeleted = 0 AND u2.isDeleted = 0",
                new { Id = userId }); ;

            return frieds;
        }
    }
}