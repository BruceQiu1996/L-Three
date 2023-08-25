using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Commands
{
    [ProtoContract]
    public class ReplyAddFriendCommandResponse : CommandResponse
    {
        [ProtoMember(6)]
        public long From { get; set; }
        [ProtoMember(7)]
        public long To { get; set; }
        [ProtoMember(8)]
        public string FromName { get; set; }
        [ProtoMember(9)]
        public string ToName { get; set; }
        [ProtoMember(10)]
        public long FromAvatar { get; set; }
        [ProtoMember(11)]
        public long ToAvatar { get; set; }
        [ProtoMember(12)]
        public bool Agree { get; set; }
    }
}
