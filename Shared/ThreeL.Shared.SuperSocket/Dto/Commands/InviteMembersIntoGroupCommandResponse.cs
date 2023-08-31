using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Commands
{
    [ProtoContract]
    public class InviteMembersIntoGroupCommandResponse : CommandResponse
    {
        [ProtoMember(6)]
        public int GroupId { get; set; }
        [ProtoMember(7)]
        public long InviterId { get; set; }
    }
}
