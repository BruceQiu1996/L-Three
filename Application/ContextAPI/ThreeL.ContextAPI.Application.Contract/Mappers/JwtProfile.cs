using AutoMapper;
using ThreeL.ContextAPI.Application.Contract.Configurations;
using ThreeL.ContextAPI.Application.Contract.Dtos.User;
using ThreeL.ContextAPI.Domain.Aggregates.UserAggregate;
using ThreeL.ContextAPI.Domain.Entities;

namespace ThreeL.ContextAPI.Application.Contract.Mappers
{
    public class JwtProfile : Profile
    {
        public JwtProfile()
        {
            CreateMap<JwtOptions, JwtSetting>();
            CreateMap<UserCreationDto, User>();
        }
    }
}
