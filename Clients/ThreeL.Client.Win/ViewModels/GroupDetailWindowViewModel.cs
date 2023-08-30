using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ThreeL.Client.Shared.Configurations;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Client.Shared.Services;
using ThreeL.Client.Win.Helpers;

namespace ThreeL.Client.Win.ViewModels
{
    public class GroupDetailWindowViewModel : DetailWindowViewModel
    {
        private ObservableCollection<GroupMemberViewModel> memberViewModels;
        public ObservableCollection<GroupMemberViewModel> MemberViewModels
        {
            get { return memberViewModels; }
            set => SetProperty(ref memberViewModels, value);
        }

        public GroupDetailWindowViewModel()
        {
            MemberViewModels = new ObservableCollection<GroupMemberViewModel>();
        }

        public GroupDetailWindowViewModel FromDto(GroupRoughlyDto groupRoughlyDto)
        {
            MemberViewModels.Clear();
            Id = groupRoughlyDto.Id;
            AvatarId = groupRoughlyDto.Avatar;
            Name = groupRoughlyDto.Name;
            CreateTime = groupRoughlyDto.CreateTime;
            foreach (var item in groupRoughlyDto.Users)
            {
                foreach (var index in Enumerable.Range(0, 20))
                {
                    var vm = new GroupMemberViewModel().FromDto(item);
                    vm.IsCreator = item.Id == groupRoughlyDto.CreateBy ? true : false;

                    MemberViewModels.Add(vm);
                }
            }

            return this;
        }

        public class GroupMemberViewModel : ObservableObject
        {
            private DetailWindowViewModel detailWindowViewModel;
            public DetailWindowViewModel DetailWindowViewModel
            {
                get => detailWindowViewModel;
                set => SetProperty(ref detailWindowViewModel, value);
            }

            private bool _isUserDetailOpen;
            public bool IsUserDetailOpen
            {
                get => _isUserDetailOpen;
                set => SetProperty(ref _isUserDetailOpen, value);
            }

            public AsyncRelayCommand DisplayDetailCommand { get; set; }

            public bool IsCreator { get; set; }
            public long Id { get; set; }
            public string UserName { get; set; }
            public string ShowName => UserName.Substring(0, 1).ToUpper();

            private BitmapImage _avatar;
            public BitmapImage Avatar
            {
                get { return _avatar; }
                set => SetProperty(ref _avatar, value);
            }

            private long? avatarId;
            public long? AvatarId
            {
                get => avatarId;
                set
                {
                    if (value != null && value != avatarId)
                    {
                        App.ServiceProvider.GetRequiredService<FileHelper>().RefreshAvatarAsync(Id, value.Value, source => Avatar = source);
                    }

                    avatarId = value;
                }
            }

            public GroupMemberViewModel()
            {
                DisplayDetailCommand = new AsyncRelayCommand(DisplayDetailAsync);
            }
            private async Task DisplayDetailAsync()
            {
                var user = await App.ServiceProvider.GetRequiredService<ContextAPIService>().GetAsync<UserRoughlyDto>(string.Format(Const.USER, Id));
                if (user == null)
                    return;

                DetailWindowViewModel = new UserDetailWindowViewModel().FromDto(user);
                IsUserDetailOpen = true;
            }

            public GroupMemberViewModel FromDto(UserBriefDto userBriefDto)
            {
                Id = userBriefDto.Id;
                AvatarId = userBriefDto.Avatar;
                UserName = userBriefDto.UserName;

                return this;
            }
        }
    }
}
