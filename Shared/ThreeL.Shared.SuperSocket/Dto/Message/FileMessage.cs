using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class FileMessage : FromToMessage
    {
        [ProtoMember(4)]
        public long FileId { get; set; }
    }
}
