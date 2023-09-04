using ProtoBuf;
using ThreeL.Infra.Core.Metadata;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class ImageMessageResponse : FromToMessageResponse
    {
        [ProtoMember(10)]
        public ImageType ImageType { get; set; }
        [ProtoMember(11)]
        public string RemoteUrl { get; set; }//just for network image
        [ProtoMember(12)]
        public long FileId { get; set; }
        [ProtoMember(13)]
        public string FileName { get; set; }
    }
}
