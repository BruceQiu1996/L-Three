using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    [ProtoContract]
    public class FileMessage : AbstractMessage
    {
        [ProtoMember(4)]
        public long FileId { get; set; }
        [ProtoMember(5)]
        public long From { get; set; }
        [ProtoMember(6)]
        public long To { get; set; }
    }
}
