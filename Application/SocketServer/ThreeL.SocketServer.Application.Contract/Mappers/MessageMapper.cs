using AutoMapper;
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
        }
    }
}
