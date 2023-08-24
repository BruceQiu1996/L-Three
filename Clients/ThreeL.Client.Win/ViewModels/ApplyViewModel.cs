using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ThreeL.Client.Shared.Configurations;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Client.Shared.Services;

namespace ThreeL.Client.Win.ViewModels
{
    public class ApplyViewModel : ObservableObject
    {
        private string keyword;
        public string Keyword
        {
            get => keyword;
            set => SetProperty(ref keyword, value);
        }

        private ObservableCollection<UserApplyViewModel> users;
        public ObservableCollection<UserApplyViewModel> Users
        {
            get => users;
            set => SetProperty(ref users, value);
        }

        private bool hadSearchResult;
        public bool HadSearchResult
        {
            get => hadSearchResult;
            set => SetProperty(ref hadSearchResult, value);
        }

        public AsyncRelayCommand SearchCommandAsync { get; set; }
        public RelayCommand CloseApplyPageCommand { get; set; }

        private readonly ContextAPIService _contextAPIService;
        public ApplyViewModel(ContextAPIService contextAPIService)
        {
            _contextAPIService = contextAPIService;
            SearchCommandAsync = new AsyncRelayCommand(SearchAsync);
            CloseApplyPageCommand = new RelayCommand(CloseApplyPage);
            Users = new ObservableCollection<UserApplyViewModel>();
            HadSearchResult = false;
        }

        private async Task SearchAsync()
        {
            if (string.IsNullOrEmpty(keyword.Trim())) 
            {
                return;
            }
            Users.Clear();
            var users = await _contextAPIService
                .GetAsync<IEnumerable<UserRoughlyDto>>(string.Format(Const.FIND_USER, Keyword.Trim()));

            if (users == null || users.Count() <= 0)
            {
                HadSearchResult = false;
            }
            else
            {
                Users = new ObservableCollection<UserApplyViewModel>(users.Select(x => new UserApplyViewModel()
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Role = x.Role,
                    IsFriend = x.IsFriend,
                    AvatarId = x.Avatar
                }));

                HadSearchResult = true;
            }
        }

        private void CloseApplyPage() 
        {
            WeakReferenceMessenger.Default.Send<string, string>("chat", "switch-page");
        }
    }
}
