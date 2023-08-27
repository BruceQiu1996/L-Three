using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Net;
using ThreeL.Client.Mobile.Helper;
using ThreeL.Client.Mobile.Pages;
using ThreeL.Client.Shared.Configurations;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Client.Shared.Entities;
using ThreeL.Client.Shared.Services;

namespace ThreeL.Client.Mobile.ViewModels
{
    public class LoginViewModel : ObservableObject
    {
        public string _userName;
        public string UserName
        {
            get { return _userName; }
            set { SetProperty(ref _userName, value); }
        }

        public string _password;
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        public AsyncRelayCommand LoginCommandAsync { get; set; }

        private readonly ContextAPIService _contextAPIService;
        private readonly WarningHelper _warningHelper;
        private readonly MainPage _mainPage;
        public LoginViewModel(ContextAPIService contextAPIService, 
                              WarningHelper warningHelper, 
                              MainPage mainPage)
        {
            _contextAPIService = contextAPIService;
            _mainPage = mainPage;
            _warningHelper = warningHelper;
            LoginCommandAsync = new AsyncRelayCommand(LoginAsync);
            UserName = "bruce";
            Password = "123456";
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password))
                return;

            try
            {
                var data = await _contextAPIService.PostAsync<UserLoginResponseDto>(Const.LOGIN, new UserLoginDto
                {
                    UserName = UserName,
                    Password = Password,
#if ANDROID
                    Origin = "android"
#endif
#if IOS
                Origin = "ios"
#endif
                });

                if (data != null)
                {
                    App.UserProfile = new UserProfile()
                    {
                        UserId = data.UserId,
                        UserName = data.UserName,
                        RefreshToken = data.RefreshToken,
                        AccessToken = data.AccessToken,
                        Role = data.Role,
                        Avatar = data.Avatar,
                        AvatarId = data.AvatarId,
                    };

                    Application.Current.MainPage = _mainPage;
                }
                else 
                {
                    await _warningHelper.Warning("用户名或者密码不存在");
                }
            }
            catch (Exception ex) 
            {
                await _warningHelper.Warning(ex.Message);
            }
        }

        private Task ExcuteWhileBadRequestAsync(string message)
        {
            return Task.CompletedTask;
        }
    }
}
