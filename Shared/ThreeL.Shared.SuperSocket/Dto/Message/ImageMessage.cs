using ProtoBuf;
using ThreeL.Infra.Core.Metadata;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class ImageMessage : FromToMessage
    {
        [ProtoMember(6)]
        public ImageType ImageType { get; set; }
        [ProtoMember(7)]
        public string RemoteUrl { get; set; }
        [ProtoMember(8)]
        public long FileId { get; set; }
        [ProtoMember(9)]
        public string FileName { get; set; }
    }
}
