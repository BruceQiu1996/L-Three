using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class FileMessage : FromToMessage
    {
        [ProtoMember(6)]
        public long FileId { get; set; }
    }
}
