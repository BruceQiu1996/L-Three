using ProtoBuf;
using ThreeL.Infra.Core.Metadata;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class ImageMessageResponse : FromToMessageResponse
    {
        [ProtoMember(8)]
        public ImageType ImageType { get; set; }
        [ProtoMember(9)]
        public string RemoteUrl { get; set; }//just for network image
        [ProtoMember(10)]
        public long FileId { get; set; }
        [ProtoMember(11)]
        public string FileName { get; set; }
    }
}
