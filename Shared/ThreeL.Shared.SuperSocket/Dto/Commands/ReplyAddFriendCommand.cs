using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Commands
{
    [ProtoContract]
    public class ReplyAddFriendCommand : AbstractMessage
    {
        [ProtoMember(2)]
        public long ApplyId { get; set; }
        [ProtoMember(3)]
        public bool Agree { get; set; }
    }
}
