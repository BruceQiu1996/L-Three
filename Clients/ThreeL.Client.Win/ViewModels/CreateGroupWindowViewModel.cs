using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Threading.Tasks;
using ThreeL.Client.Shared.Configurations;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Client.Shared.Services;
using ThreeL.Client.Win.Helpers;

namespace ThreeL.Client.Win.ViewModels
{
    public class CreateGroupWindowViewModel : ObservableObject
    {
        private string _groupName;
        public string GroupName
        {
            get => _groupName;
            set
            {
                SetProperty(ref _groupName, value);
            }
        }

        public AsyncRelayCommand CreateGroupRelayCommandAsync { get; set; }

        private readonly ContextAPIService _contextAPIService;
        private readonly GrowlHelper _growlHelper;
        public CreateGroupWindowViewModel(ContextAPIService contextAPIService, GrowlHelper growlHelper)
        {
            _contextAPIService = contextAPIService;
            _growlHelper = growlHelper;
            CreateGroupRelayCommandAsync = new AsyncRelayCommand(CreateGroupRelayAsync);
        }

        private async Task CreateGroupRelayAsync()
        {
            if (string.IsNullOrEmpty(GroupName) || GroupName.Trim().Length <= 2 || GroupName.Trim().Length > 10)
            {
                return;
            }

            var result = await _contextAPIService.PostAsync<GroupCreationResponseDto>(string.Format(Const.GROUP_CREATION,GroupName), null);
            if (result == null)
                return;

            _growlHelper.Success("创建群聊成功");
            WeakReferenceMessenger.Default.Send<GroupCreationResponseDto, string>(result, "message-newgroup");
        }
    }
}
