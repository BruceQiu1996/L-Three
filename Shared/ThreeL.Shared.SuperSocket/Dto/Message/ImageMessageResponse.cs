using ProtoBuf;
using ThreeL.Infra.Core.Metadata;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class ImageMessageResponse : MessageResponse
    {
        [ProtoMember(6)]
        public ImageType ImageType { get; set; }
        [ProtoMember(7)]
        public string RemoteUrl { get; set; }//just for network image
        [ProtoMember(8)]
        public long FileId { get; set; }
        [ProtoMember(9)]
        public long From { get; set; }
        [ProtoMember(10)]
        public long To { get; set; }
        [ProtoMember(11)]
        public string FileName { get; set; }
    }
}
