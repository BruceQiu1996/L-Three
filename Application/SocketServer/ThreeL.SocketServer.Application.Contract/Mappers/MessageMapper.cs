using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using ThreeL.Shared.SuperSocket.Dto.Message;

namespace ThreeL.SocketServer.Application.Contract.Mappers
{
    public class MessageProfile : Profile
    {
        public MessageProfile()
        {
            CreateMap<TextMessage, TextMessageResponse>();
            CreateMap<ImageMessage, ImageMessageResponse>();
            CreateMap<FileMessage, FileMessageResponse>();
            //转换各resp到proto类型
            CreateMap<TextMessageResponse, ChatRecordPostRequest>().ForMember(x => x.SendTime,
                y => y.MapFrom(src => Timestamp.FromDateTime(src.SendTime.ToUniversalTime())))
                .ForMember(x => x.Message, y => y.MapFrom(src => src.Text));

            CreateMap<ImageMessageResponse, ChatRecordPostRequest>().ForMember(x => x.SendTime,
                y => y.MapFrom(src => Timestamp.FromDateTime(src.SendTime.ToUniversalTime())))
                 .ForMember(x => x.Message, y => y.MapFrom(src => src.RemoteUrl == null ? string.Empty : src.RemoteUrl))
                 .ForMember(x => x.ImageType, y => y.MapFrom(src => (int)src.ImageType));

            CreateMap<FileMessageResponse, ChatRecordPostRequest>().ForMember(x => x.SendTime,
                y => y.MapFrom(src => Timestamp.FromDateTime(src.SendTime.ToUniversalTime())))
                 .ForMember(x => x.Message, y => y.MapFrom(src => string.Empty));
        }
    }
}
