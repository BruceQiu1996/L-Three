using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class FileMessageResponse : MessageResponse
    {
        [ProtoMember(6)]
        public string FileName { get; set; }
        [ProtoMember(7)]
        public long Size { get; set; }
        [ProtoMember(8)]
        public long From { get; set; }
        [ProtoMember(9)]
        public long To { get; set; }
        [ProtoMember(10)]
        public long FileId { get; set; }
    }
}
