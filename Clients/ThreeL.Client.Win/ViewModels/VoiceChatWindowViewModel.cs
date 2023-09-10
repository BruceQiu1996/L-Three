using CommunityToolkit.Mvvm.ComponentModel;

namespace ThreeL.Client.Win.ViewModels
{
    public class VoiceChatWindowViewModel : ObservableObject
    {
        private RelationViewModel _current;
        public RelationViewModel Current
        {
            get => _current;
            set => SetProperty(ref _current, value);
        }

        public string VoiceChatKey { get; set; }
        public VoiceChatWindowViewModel()
        {
            
        }
    }
}
