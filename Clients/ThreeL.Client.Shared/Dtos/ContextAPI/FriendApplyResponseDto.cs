using System.ComponentModel;

namespace ThreeL.Client.Shared.Dtos.ContextAPI
{
    public class FriendApplyResponseDto
    {
        public long Id { get; set; }
        public long ActiverId { get; set; }
        public string ActiverName { get; set; }
        public long PassiverId { get; set; }
        public string PassiverName { get; set; }
        public bool FromSelf { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ProcessTime { get; set; }
        public FriendApplyStatus Status { get; set; }
    }

    public enum FriendApplyStatus : byte
    {
        [Description("待处理")]
        TobeProcessed,
        [Description("已接受")]
        Accept,
        [Description("已拒绝")]
        Reject
    }
}
