using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class FileMessageResponse : FromToMessageResponse
    {
        [ProtoMember(9)]
        public string FileName { get; set; }
        [ProtoMember(10)]
        public long Size { get; set; }
        [ProtoMember(11)]
        public long FileId { get; set; }
    }
}
