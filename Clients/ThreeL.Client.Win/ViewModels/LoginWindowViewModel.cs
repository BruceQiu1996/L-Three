using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dapper;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Extensions.DependencyInjection;
using System;

using System.Threading.Tasks;
using ThreeL.Client.Shared.Configurations;
using ThreeL.Client.Shared.Database;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Client.Shared.Entities;
using ThreeL.Client.Shared.Services;
using ThreeL.Client.Win.Helpers;

namespace ThreeL.Client.Win.ViewModels
{
    public class LoginWindowViewModel : ObservableObject
    {
        private readonly ContextAPIService _contextAPIService;
        private readonly ClientSqliteContext _clientSqliteContext;
        private readonly GrowlHelper _growlHelper;
        public LoginWindowViewModel(ContextAPIService contextAPIService,
                                    GrowlHelper growlHelper,
                                    ClientSqliteContext clientSqliteContext)
        {
            UserName = "Bruce";
            _growlHelper = growlHelper;
            _contextAPIService = contextAPIService;
            _clientSqliteContext = clientSqliteContext;
            _contextAPIService.TryRefreshTokenAsync = RefreshTokenAsync;
            _contextAPIService.ExcuteWhileUnauthorizedAsync = ExcuteWhileUnauthorizedAsync;
            _contextAPIService.ExcuteWhileBadRequestAsync = ExcuteWhileBadRequestAsync;
            LoginCommandAsync = new AsyncRelayCommand<PasswordBox>(LoginAsync);
        }

        public string _userName;
        public string UserName
        {
            get { return _userName; }
            set { SetProperty(ref _userName, value); }
        }

        public AsyncRelayCommand<PasswordBox> LoginCommandAsync { get; set; }

        private async Task LoginAsync(PasswordBox password)
        {
            password.Password = "123456";
            var data = await _contextAPIService.PostAsync<UserLoginResponseDto>(Const.LOGIN, new UserLoginDto
            {
                UserName = UserName,
                Password = password.Password,
                Origin = "win"
            });

            if (data != null)
            {
                _contextAPIService.SetToken($"{data.AccessToken}");
                var user = await SqlMapper.QueryFirstOrDefaultAsync<UserProfile>(_clientSqliteContext.dbConnection, $"select * from userprofile where userId = {data.UserId}");
                if (user == null)
                {
                    await SqlMapper.ExecuteAsync(_clientSqliteContext.dbConnection, $"insert into userprofile (UserId,UserName,Role,AccessToken,RefreshToken) values (@UserId,@UserName,@Role,@AccessToken,@RefreshToken)", data);
                }
                else
                {
                    await SqlMapper.ExecuteAsync(_clientSqliteContext.dbConnection, $"update userprofile set UserName = @UserName,Role =@Role,AccessToken = @AccessToken, RefreshToken = @RefreshToken where UserId = @UserId", data);
                }

                await SqlMapper.ExecuteAsync(_clientSqliteContext.dbConnection, $"update userprofile set LastLoginTime = @LastLoginTime where UserId = @UserId", new
                {
                    LastLoginTime = DateTime.Now,
                    data.UserId
                });

                App.UserProfile = new UserProfile()
                {
                    UserId = data.UserId,
                    UserName = data.UserName,
                    RefreshToken = data.RefreshToken,
                    AccessToken = data.AccessToken
                };
                App.ServiceProvider.GetRequiredService<MainWindow>().Show();
                App.ServiceProvider.GetRequiredService<Login>().Close();
            }
        }

        private async Task<bool> RefreshTokenAsync()
        {
            if (string.IsNullOrEmpty(App.UserProfile?.RefreshToken) || string.IsNullOrEmpty(App.UserProfile?.AccessToken)) 
            {
                return false;
            }

            var token = await _contextAPIService.RefreshTokenAsync<UserRefreshTokenDto>(new UserRefreshTokenDto
            {
                Origin = "win",
                AccessToken = App.UserProfile.AccessToken,
                RefreshToken =App.UserProfile.RefreshToken 
            });

            if (token == null) 
            {
                return false;
            }

            App.UserProfile.RefreshToken = token.RefreshToken;
            App.UserProfile.AccessToken = token.AccessToken;

            return true;
        }

        private async Task ExcuteWhileUnauthorizedAsync()
        {
            //退出到登陆界面，并且断开所有socket连接
        }

        private Task ExcuteWhileBadRequestAsync(string message)
        {
            _growlHelper.Warning(message);

            return Task.CompletedTask;
        }
    }
}
