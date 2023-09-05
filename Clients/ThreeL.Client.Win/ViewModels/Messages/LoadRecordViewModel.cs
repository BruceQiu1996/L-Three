using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.ViewModels.Messages
{
    public class LoadRecordViewModel : MessageViewModel
    {
        public RelationViewModel Relation { get; set; }
        public LoadRecordViewModel() : base(MessageType.LoadRecord)
        {

        }


    }
}
