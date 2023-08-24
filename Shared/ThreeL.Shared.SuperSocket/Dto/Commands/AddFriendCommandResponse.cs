using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Commands
{
    [ProtoContract]
    public class AddFriendCommandResponse : CommandResponse
    {
        [ProtoMember(6)]
        public long From { get; set; }
        [ProtoMember(7)]
        public long To { get; set; }
    }
}
