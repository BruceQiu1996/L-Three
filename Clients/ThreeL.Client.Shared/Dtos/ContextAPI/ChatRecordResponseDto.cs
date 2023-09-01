using ThreeL.Infra.Core.Metadata;

namespace ThreeL.Client.Shared.Dtos.ContextAPI
{
    public class ChatRecordResponseDto
    {
        public string MessageId { get; set; }
        public string Message { get; set; }
        public MessageRecordType MessageRecordType { get; set; }
        public ImageType ImageType { get; set; }
        public DateTime SendTime { get; set; }
        public long From { get; set; }
        public string FromName { get; set; }
        public long To { get; set; }
        public long? FileId { get; set; }
        public string FileName { get; set; }
        public long Size { get; set; }
        public byte[] Bytes { get; set; } //网络图片
        public bool Withdrawed { get; set; }
        public bool IsGroup { get; set; }
    }
}
