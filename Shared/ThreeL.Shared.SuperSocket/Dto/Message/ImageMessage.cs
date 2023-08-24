using ProtoBuf;
using ThreeL.Infra.Core.Metadata;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class ImageMessage : FromToMessage
    {
        [ProtoMember(5)]
        public ImageType ImageType { get; set; }
        [ProtoMember(6)]
        public string RemoteUrl { get; set; }
        [ProtoMember(7)]
        public long FileId { get; set; }
        [ProtoMember(8)]
        public string FileName { get; set; }
    }
}
