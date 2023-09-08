using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Commands
{
    [ProtoContract]
    public class OfflineCommand : AbstractMessage
    {
        [ProtoMember(2)]
        public string Reason { get; set; }
    }
}
