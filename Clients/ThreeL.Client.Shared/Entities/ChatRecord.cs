﻿using ThreeL.Infra.Core.Metadata;

namespace ThreeL.Client.Shared.Entities
{
    public class ChatRecord
    {
        public string MessageId { get; set; }
        public string Message { get; set; }
        public MessageRecordType MessageRecordType { get; set; }
        public ImageType ImageType { get; set; }
        public DateTime SendTime { get; set; }
        public long From { get; set; }
        public long To { get; set; }
        public string ResourceLocalLocation { get; set; } //资源本地的路径
        public long? FileId { get; set; } //资源远端的标识符
        public long? ResourceSize { get; set; }
    }
}
