using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Commands
{
    [ProtoContract]
    public class InviteMembersIntoGroupCommandResponse : CommandResponse
    {
        [ProtoMember(4)]
        public int GroupId { get; set; }
        [ProtoMember(5)]
        public string GroupName { get; set; }
        [ProtoMember(6)]
        public long? GroupAvatar { get; set; }
        [ProtoMember(7)]
        public long InviterId { get; set; }
    }
}
