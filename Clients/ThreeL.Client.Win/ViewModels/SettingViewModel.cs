using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using ThreeL.Client.Win.Pages;

namespace ThreeL.Client.Win.ViewModels
{
    public class SettingViewModel : ObservableObject
    {
        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public AsyncRelayCommand UploadAvatarCommandAsync { get; set; }

        public SettingViewModel()
        {
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            UploadAvatarCommandAsync = new AsyncRelayCommand(UploadAvatarAsync);
        }

        private async Task LoadAsync() 
        {
            
        }

        private async Task UploadAvatarAsync() 
        {
            var avatar = App.ServiceProvider.GetRequiredService<Setting>().avatar;

        }
    }
}
