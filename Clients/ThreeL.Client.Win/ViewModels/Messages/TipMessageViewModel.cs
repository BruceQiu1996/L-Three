using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.ViewModels.Messages
{
    public class TipMessageViewModel : MessageViewModel
    {
        public string Content { get; set; }
        public TipMessageViewModel() : base(MessageType.Tip)
        {
        }

        public override string GetShortDesc()
        {
            return "[消息提示]";
        }
    }
}
