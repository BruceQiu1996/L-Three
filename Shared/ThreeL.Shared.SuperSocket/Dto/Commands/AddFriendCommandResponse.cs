using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Commands
{
    [ProtoContract]
    public class AddFriendCommandResponse : CommandResponse
    {
        [ProtoMember(4)]
        public long From { get; set; }
        [ProtoMember(5)]
        public long To { get; set; }
    }
}
