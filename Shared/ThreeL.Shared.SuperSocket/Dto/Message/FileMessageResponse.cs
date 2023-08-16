using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class FileMessageResponse : FromToMessageResponse
    {
        [ProtoMember(8)]
        public string FileName { get; set; }
        [ProtoMember(9)]
        public long Size { get; set; }
        [ProtoMember(10)]
        public long FileId { get; set; }
    }
}
