using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class ImageMessageResponse : MessageResponse
    {
        [ProtoMember(6)]
        public byte ImageType { get; set; }
        [ProtoMember(7)]
        public string Url { get; set; }
        [ProtoMember(8)]
        public byte[] ImageBytes { get; set; }
        [ProtoMember(9)]
        public long From { get; set; }
        [ProtoMember(10)]
        public long To { get; set; }
        [ProtoMember(11)]
        public string FileName { get; set; }
    }
}
