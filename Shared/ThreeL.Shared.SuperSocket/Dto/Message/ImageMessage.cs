using ProtoBuf;
using ThreeL.Infra.Core.Metadata;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class ImageMessage : FromToMessage
    {
        [ProtoMember(4)]
        public ImageType ImageType { get; set; }
        [ProtoMember(5)]
        public string RemoteUrl { get; set; }
        [ProtoMember(6)]
        public long FileId { get; set; }
    }
}
