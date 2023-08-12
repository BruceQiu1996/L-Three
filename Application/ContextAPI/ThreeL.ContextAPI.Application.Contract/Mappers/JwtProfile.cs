using AutoMapper;
using ThreeL.ContextAPI.Application.Contract.Configurations;
using ThreeL.ContextAPI.Domain.Entities;

namespace ThreeL.ContextAPI.Application.Contract.Mappers
{
    public class JwtProfile : Profile
    {
        public JwtProfile()
        {
            CreateMap<JwtOptions, JwtSetting>();
        }
    }
}
