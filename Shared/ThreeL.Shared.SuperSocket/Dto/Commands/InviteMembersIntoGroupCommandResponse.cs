using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Commands
{
    [ProtoContract]
    public class InviteMembersIntoGroupCommandResponse : CommandResponse
    {
        [ProtoMember(6)]
        public int GroupId { get; set; }
        [ProtoMember(7)]
        public string GroupName { get; set; }
        [ProtoMember(8)]
        public long? GroupAvatar { get; set; }
        [ProtoMember(9)]
        public long InviterId { get; set; }
    }
}
