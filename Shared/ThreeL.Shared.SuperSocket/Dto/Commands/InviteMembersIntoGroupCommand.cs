using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Commands
{
    [ProtoContract]
    public class InviteMembersIntoGroupCommand : AbstractMessage
    {
        [ProtoMember(4)]
        public string Friends { get; set; }
        [ProtoMember(5)]
        public int GroupId { get; set; }
    }
}
