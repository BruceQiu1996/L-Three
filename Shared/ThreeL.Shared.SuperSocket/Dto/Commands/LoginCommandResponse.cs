﻿using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Commands
{
    [ProtoContract]
    public class LoginCommandResponse : CommandResponse
    {
        [ProtoMember(6)]
        public string SsToken { get; set; }
    }
}
