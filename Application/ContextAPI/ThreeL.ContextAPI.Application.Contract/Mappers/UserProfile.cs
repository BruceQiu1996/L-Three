using AutoMapper;
using ThreeL.ContextAPI.Application.Contract.Dtos.User;
using ThreeL.ContextAPI.Application.Contract.Protos;
using ThreeL.ContextAPI.Domain.Aggregates.UserAggregate;
using ThreeL.Infra.Core.Metadata;

namespace ThreeL.ContextAPI.Application.Contract.Mappers
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserCreationDto, User>();
            CreateMap<Group, GroupRoughlyDto>();
            CreateMap<ChatRecordPostRequest, ChatRecord>().ForMember(x => x.SendTime,
                y => y.MapFrom(src => src.SendTime.ToDateTime().ToLocalTime()))
                .ForMember(x => x.MessageRecordType,
                y => y.MapFrom(src => (MessageRecordType)src.MessageRecordType))
            .ForMember(x => x.ImageType,
                y => y.MapFrom(src => (ImageType)src.ImageType))
            .ForMember(x => x.Message,
                y => y.MapFrom(src => string.IsNullOrEmpty(src.Message) ? null : src.Message));

            CreateMap<Group, GroupCreationResponseDto>();
        }
    }
}
