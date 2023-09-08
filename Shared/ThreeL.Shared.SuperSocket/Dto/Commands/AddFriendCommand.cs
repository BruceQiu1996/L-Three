﻿using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Commands
{
    [ProtoContract]
    public class AddFriendCommand : AbstractMessage
    {
        [ProtoMember(2)]
        public long FriendId { get; set; }
    }
}
