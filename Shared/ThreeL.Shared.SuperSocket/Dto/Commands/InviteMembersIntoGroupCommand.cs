using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Commands
{
    [ProtoContract]
    public class InviteMembersIntoGroupCommand : AbstractMessage
    {
        [ProtoMember(2)]
        public string Friends { get; set; }
        [ProtoMember(3)]
        public int GroupId { get; set; }
    }
}
