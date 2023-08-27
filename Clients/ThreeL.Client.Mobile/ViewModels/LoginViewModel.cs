using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThreeL.Client.Mobile.Helper;
using ThreeL.Client.Shared.Configurations;
using ThreeL.Client.Shared.Dtos.ContextAPI;
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
        public LoginViewModel(ContextAPIService contextAPIService, WarningHelper warningHelper)
        {
            _contextAPIService = contextAPIService;
            _warningHelper = warningHelper;
            LoginCommandAsync = new AsyncRelayCommand(LoginAsync);
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
