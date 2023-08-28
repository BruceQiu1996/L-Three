using ThreeL.Infra.Core.Metadata;

namespace ThreeL.ContextAPI.Application.Contract.Dtos.Relation
{
    public class ChatRecordResponseDto
    {
        public string MessageId { get; set; }
        public string Message { get; set; }
        public MessageRecordType MessageRecordType { get; set; }
        public ImageType ImageType { get; set; }
        public DateTime SendTime { get; set; }
        public long From { get; set; }
        public long To { get; set; }
        public long? FileId { get; set; }
        public string FileName { get; set; }
        public long Size { get; set; }
        public bool Withdrawed { get; set; }
        //群聊时候至少知道对方名字
        public string FromName { get; set; }

        public ChatRecordResponseDto ClearDataByWithdrawed()
        {
            if (Withdrawed)
            {
                Message = null;
                FileId = null;
                FileName = null;
                Size = 0;
            }

            return this;
        }
    }
}
