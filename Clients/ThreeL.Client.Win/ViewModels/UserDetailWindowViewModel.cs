using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using ThreeL.Client.Shared.Services;

namespace ThreeL.Client.Win.ViewModels
{
    public class UserDetailWindowViewModel : ObservableObject
    {
        public RelationViewModel RelationViewModel { get; set; }
        private readonly ContextAPIService _contextAPIService;

        public AsyncRelayCommand LoadedRelayCommandAsync { get; set; }
        public UserDetailWindowViewModel(ContextAPIService contextAPIService)
        {
            _contextAPIService = contextAPIService;
            LoadedRelayCommandAsync = new AsyncRelayCommand(LoadedRelayAsync);
        }

        private async Task LoadedRelayAsync() 
        {
            
        }
    }
}
