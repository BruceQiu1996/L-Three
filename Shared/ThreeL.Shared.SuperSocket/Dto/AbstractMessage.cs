using ProtoBuf;
using ThreeL.Shared.SuperSocket.Dto.Commands;
using ThreeL.Shared.SuperSocket.Dto.Message;

namespace ThreeL.Shared.SuperSocket.Dto
{
    [ProtoContract]
    [ProtoInclude(90, typeof(CommandResponse))]
    [ProtoInclude(95, typeof(MessageResponse))]
    [ProtoInclude(100, typeof(FromToMessage))]
    [ProtoInclude(500, typeof(LoginCommand))]
    [ProtoInclude(700, typeof(RequestForUserEndpointCommand))]
    [ProtoInclude(600, typeof(AddFriendCommand))]
    [ProtoInclude(800, typeof(ReplyAddFriendCommand))]
    [ProtoInclude(900, typeof(InviteMembersIntoGroupCommand))]

    public abstract class AbstractMessage : IMessage
    {
        [ProtoMember(1)]
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        [ProtoMember(2)]
        public DateTime SendTime { get; set; }
        [ProtoMember(3)]
        public DateTime? ReceiveTime { get; set; }
    }
}
