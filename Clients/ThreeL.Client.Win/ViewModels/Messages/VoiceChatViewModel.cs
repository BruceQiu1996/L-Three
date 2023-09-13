using Microsoft.Extensions.DependencyInjection;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Client.Win.Helpers;
using ThreeL.Infra.Core.Metadata;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.ViewModels.Messages
{
    public class VoiceChatViewModel : MessageViewModel
    {
        public VoiceChatStatus VoiceChatStatus { get; set; }
        public int? ChatTimes { get; set; }

        public string DisplayText => VoiceChatStatus switch
        {
            VoiceChatStatus.Finished => $"通话完成 {App.ServiceProvider.GetRequiredService<DateTimeHelper>().SecondConvertTime(ChatTimes.Value)}",
            VoiceChatStatus.Rejected => "已拒绝",
            VoiceChatStatus.Canceled => "已取消",
            VoiceChatStatus.NotAccept => "未接听",
            _ => "通话异常"
        };

        public VoiceChatViewModel() : base(MessageType.VoiceChat)
        {
            CanCopy = false;
            CanWithdraw = false;
            CanOpenLocation = false;
        }

        public override string GetShortDesc()
        {
            return "[语音通话]";
        }

        public override void FromDto(ChatRecordResponseDto chatRecord)
        {
            base.FromDto(chatRecord);
            VoiceChatStatus = (VoiceChatStatus)System.Enum.Parse(typeof(VoiceChatStatus), chatRecord.Message);
        }
    }
}
