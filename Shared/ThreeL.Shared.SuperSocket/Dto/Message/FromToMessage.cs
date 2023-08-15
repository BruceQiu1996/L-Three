﻿using ProtoBuf;

namespace ThreeL.Shared.SuperSocket.Dto.Message
{
    /// <summary>
    /// 所有收发消息的基类
    /// </summary>
    [ProtoContract]
    [ProtoInclude(6100, typeof(TextMessage))]
    [ProtoInclude(6200, typeof(ImageMessage))]
    [ProtoInclude(6300, typeof(FileMessage))]
    public abstract class FromToMessage : AbstractMessage
    {
        [ProtoMember(4)]
        public long From { get; set; }
        [ProtoMember(5)]
        public long To { get; set; }
    }
}
