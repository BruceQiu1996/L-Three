﻿using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Commands
{
    [ProtoContract]
    [ProtoInclude(5100, typeof(LoginCommandResponse))]
    [ProtoInclude(5200, typeof(RequestForUserEndpointCommand))]
    [ProtoInclude(5300, typeof(AddFriendCommandResponse))]
    public abstract class CommandResponse : AbstractMessage
    {
        [ProtoMember(4)]
        public bool Result { get; set; }
        [ProtoMember(5)]
        public string Message { get; set; }
    }
}
