using ThreeL.Shared.SuperSocket.Metadata;
namespace ThreeL.Client.Win.ViewModels.Messages
{
    public class TimeMessageViewModel : MessageViewModel
    {
        public TimeMessageViewModel() : base(MessageType.Time)
        {
            
        }

        public string DateTime { get; set; }
    }
}
