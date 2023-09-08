using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Commands
{
    [ProtoContract]
    public class ReplyAddFriendCommandResponse : CommandResponse
    {
        [ProtoMember(4)]
        public long From { get; set; }
        [ProtoMember(5)]
        public long To { get; set; }
        [ProtoMember(6)]
        public string FromName { get; set; }
        [ProtoMember(7)]
        public string ToName { get; set; }
        [ProtoMember(8)]
        public long FromAvatar { get; set; }
        [ProtoMember(9)]
        public long ToAvatar { get; set; }
        [ProtoMember(10)]
        public bool Agree { get; set; }
    }
}
