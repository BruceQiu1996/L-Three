using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Client.Shared.Services;
using ThreeL.Client.Win.Helpers;

namespace ThreeL.Client.Win.ViewModels
{
    public class ChatViewModel : ObservableObject
    {
        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public AsyncRelayCommand SelectFriendCommandAsync { get; set; }

        private ObservableCollection<FriendViewModel> friendViewModels;
        public ObservableCollection<FriendViewModel> FriendViewModels
        {
            get => friendViewModels;
            set => SetProperty(ref friendViewModels, value);
        }

        private FriendViewModel friendViewModel;
        public FriendViewModel FriendViewModel
        {
            get => friendViewModel;
            set => SetProperty(ref friendViewModel, value);
        }

        private readonly ContextAPIService _contextAPIService;
        private readonly GrowlHelper _growlHelper;
        public ChatViewModel(ContextAPIService contextAPIService, GrowlHelper growlHelper)
        {
            _contextAPIService = contextAPIService;
            _growlHelper = growlHelper;
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            //SelectFriendCommandAsync = new AsyncRelayCommand(SelectFriendAsync);
        }

        private async Task LoadAsync() 
        {
            //TODO删除代码
            try
            {
                //获取好友列表
                var resp = await _contextAPIService.GetAsync<IEnumerable<FriendDto>>("relations/friends");
                if (resp != null)
                {
                    List<FriendViewModel> friends = new List<FriendViewModel>();
                    foreach (var index in Enumerable.Range(0,20))
                    {
                        //处理得到好友列表
                        foreach (var dto in resp)
                        {
                            if (dto.ActiverId == 2)
                            {
                                friends.Add(new FriendViewModel
                                {
                                    Id = dto.PassiverId,
                                    UserName = dto.PassiverName,
                                    Remark = dto.PassiverRemark
                                });
                            }
                            else if (dto.PassiverId == 2)
                            {
                                friends.Add(new FriendViewModel
                                {
                                    Id = dto.ActiverId,
                                    UserName = dto.ActiverName,
                                    Remark = dto.ActiverRemark
                                });
                            }
                        }
                    }
                    //加载好友列表
                    FriendViewModels = new ObservableCollection<FriendViewModel>(friends);
                }
            }
            catch (Exception ex)
            {
                _growlHelper.Warning(ex.Message);
            }
        }
    }
}
