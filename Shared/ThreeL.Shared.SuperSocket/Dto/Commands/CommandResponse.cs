using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Commands
{
    [ProtoContract]
    [ProtoInclude(5100, typeof(LoginCommandResponse))]
    [ProtoInclude(5200, typeof(RequestForUserEndpointCommand))]
    [ProtoInclude(5300, typeof(AddFriendCommandResponse))]
    [ProtoInclude(5400, typeof(ReplyAddFriendCommandResponse))]
    [ProtoInclude(5500, typeof(InviteMembersIntoGroupCommandResponse))]
    public abstract class CommandResponse : AbstractMessage
    {
        [ProtoMember(2)]
        public bool Result { get; set; }
        [ProtoMember(3)]
        public string Message { get; set; }
    }
}
