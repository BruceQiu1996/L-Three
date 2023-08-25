using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Commands
{
    [ProtoContract]
    public class ReplyAddFriendCommand : AbstractMessage
    {
        [ProtoMember(4)]
        public long ApplyId { get; set; }
        [ProtoMember(5)]
        public bool Agree { get; set; }
    }
}
