using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dapper;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using ThreeL.Client.Shared.Configurations;
using ThreeL.Client.Shared.Database;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Client.Shared.Entities;
using ThreeL.Client.Shared.Services;
using ThreeL.Client.Win.Helpers;
using ThreeL.Shared.SuperSocket.Client;

namespace ThreeL.Client.Win.ViewModels
{
    public class LoginWindowViewModel : ObservableObject
    {
        private readonly ContextAPIService _contextAPIService;
        private readonly ClientSqliteContext _clientSqliteContext;
        private readonly GrowlHelper _growlHelper;
        private readonly CustomerSettings _customerSettings;
        private readonly TcpSuperSocketClient _tcpSuperSocketClient;
        public LoginWindowViewModel(ContextAPIService contextAPIService,
                                    GrowlHelper growlHelper,
                                    CustomerSettings customerSettings,
                                    TcpSuperSocketClient tcpSuperSocketClient,
                                    ClientSqliteContext clientSqliteContext)
        {
            _growlHelper = growlHelper;
            _customerSettings = customerSettings;
            _contextAPIService = contextAPIService;
            _clientSqliteContext = clientSqliteContext;
            _tcpSuperSocketClient = tcpSuperSocketClient;
            _contextAPIService.TryRefreshTokenAsync = RefreshTokenAsync;
            _contextAPIService.ExcuteWhileUnauthorizedAsync = ExcuteWhileUnauthorizedAsync;
            _contextAPIService.ExcuteWhileBadRequestAsync = ExcuteWhileBadRequestAsync;
            LoginCommandAsync = new AsyncRelayCommand<PasswordBox>(LoginAsync);
            LoadedCommandAsync = new AsyncRelayCommand(LoadedAsync);
        }

        public string _userName;
        public string UserName
        {
            get { return _userName; }
            set { SetProperty(ref _userName, value); }
        }

        public AsyncRelayCommand LoadedCommandAsync { get; set; }
        public AsyncRelayCommand<PasswordBox> LoginCommandAsync { get; set; }

        private async Task LoadedAsync()
        {
            var user = await SqlMapper.QueryFirstOrDefaultAsync<UserProfile>(_clientSqliteContext.dbConnection,
                $"select * from userprofile order by LastLoginTime desc limit 1");

            UserName = user?.UserName;
        }

        private async Task LoginAsync(PasswordBox password)
        {
            password.Password = "123456";
            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(password.Password))
                return;

            var data = await _contextAPIService.PostAsync<UserLoginResponseDto>(Const.LOGIN, new UserLoginDto
            {
                UserName = UserName,
                Password = password.Password,
                Origin = "win"
            });

            if (data != null)
            {
                _contextAPIService.SetToken($"{data.AccessToken}");
                var user = await SqlMapper.QueryFirstOrDefaultAsync<UserProfile>
                    (_clientSqliteContext.dbConnection, $"select * from userprofile where userId = {data.UserId}");
                if (user == null)
                {
                    await SqlMapper.ExecuteAsync(_clientSqliteContext.dbConnection,
                        $"insert into userprofile (UserId,UserName,Role,AccessToken,RefreshToken) values (@UserId,@UserName,@Role,@AccessToken,@RefreshToken)", data);
                }
                else
                {
                    await SqlMapper.ExecuteAsync(_clientSqliteContext.dbConnection,
                        $"update userprofile set UserName = @UserName,Role =@Role,AccessToken = @AccessToken, RefreshToken = @RefreshToken where UserId = @UserId", data);
                }

                await SqlMapper.ExecuteAsync(_clientSqliteContext.dbConnection,
                    $"update userprofile set LastLoginTime = @LastLoginTime where UserId = @UserId", new
                    {
                        LastLoginTime = DateTime.Now,
                        data.UserId
                    });

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

                if (App.ServiceProvider.GetRequiredService<MainWindow>().IsLoaded)
                {
                    await App.ServiceProvider.GetRequiredService<MainWindowViewModel>().LoadAsync();
                }

                App.ServiceProvider.GetRequiredService<Login>().Hide();
                App.ServiceProvider.GetRequiredService<MainWindow>().Show();
            }
        }

        /// <summary>
        /// token失效后，通过refreshtoken重新获取token
        /// </summary>
        /// <returns></returns>
        private async Task<bool> RefreshTokenAsync()
        {
            if (string.IsNullOrEmpty(App.UserProfile?.RefreshToken) || string.IsNullOrEmpty(App.UserProfile?.AccessToken))
            {
                return false;
            }

            var token = await _contextAPIService
                .RefreshTokenAsync<UserRefreshTokenDto>(new UserRefreshTokenDto
                {
                    Origin = "win",
                    AccessToken = App.UserProfile.AccessToken,
                    RefreshToken = App.UserProfile.RefreshToken
                });

            if (token == null)
            {
                return false;
            }

            App.UserProfile.RefreshToken = token.RefreshToken;
            App.UserProfile.AccessToken = token.AccessToken;
            _contextAPIService.SetToken(App.UserProfile.AccessToken);
            //更新数据库accessToken&refreshToken
            await SqlMapper.ExecuteAsync(_clientSqliteContext.dbConnection,
                               $"update userprofile set AccessToken = @AccessToken, RefreshToken = @RefreshToken where UserId = @UserId", new
                               {
                                   token.AccessToken,
                                   token.RefreshToken,
                                   App.UserProfile.UserId
                               });
            return true;
        }

        /// <summary>
        /// 通过refreshtokn和accesstoken后仍旧过期的话
        /// </summary>
        /// <returns></returns>
        private async Task ExcuteWhileUnauthorizedAsync()
        {
            _growlHelper.Warning("登录凭证失效");
            App.UserProfile.Clear();
            //退出到登陆界面，并且断开所有socket连接
            App.ServiceProvider.GetRequiredService<MainWindow>().Hide();
            //关闭tcp连接
            await _tcpSuperSocketClient.CloseConnectionAsync();
            //停下udp监听 TODO
            App.ServiceProvider.GetRequiredService<Login>().Show();
        }

        private Task ExcuteWhileBadRequestAsync(string message)
        {
            _growlHelper.Warning(message);

            return Task.CompletedTask;
        }
    }
}
