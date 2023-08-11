using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class ImageMessage : AbstractMessage
    {
        [ProtoMember(4)]
        public byte ImageType { get; set; }
        [ProtoMember(5)]
        public string RemoteUrl { get; set; }
        [ProtoMember(6)]
        public long FileId { get; set; }
        [ProtoMember(7)]
        public long From { get; set; }
        [ProtoMember(8)]
        public long To { get; set; }
        [ProtoMember(9)]
        public string FileName { get; set; }
    }
}
