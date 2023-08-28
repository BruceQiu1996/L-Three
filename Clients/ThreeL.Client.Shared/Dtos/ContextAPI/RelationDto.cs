namespace ThreeL.Client.Shared.Dtos.ContextAPI
{
    public class RelationDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Remark { get; set; }
        public long? Avatar { get; set; }
        public  bool IsGroup { get; set; }
        public int? MemberCount { get; set; }
        public DateTime CreateTime { get; set; }
        public IEnumerable<ChatRecordResponseDto> ChatRecords { get; set; }
    }
}
