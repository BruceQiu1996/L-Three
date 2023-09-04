using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class FileMessageResponse : FromToMessageResponse
    {
        [ProtoMember(10)]
        public string FileName { get; set; }
        [ProtoMember(11)]
        public long Size { get; set; }
        [ProtoMember(12)]
        public long FileId { get; set; }
    }
}
