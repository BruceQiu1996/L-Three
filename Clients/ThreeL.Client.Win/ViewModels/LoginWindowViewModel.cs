using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dapper;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using ThreeL.Client.Shared.Database;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Client.Shared.Entities;
using ThreeL.Client.Shared.Services;

namespace ThreeL.Client.Win.ViewModels
{
    public class LoginWindowViewModel : ObservableObject
    {
        private readonly ContextAPIService _contextAPIService;
        private readonly ClientSqliteContext _clientSqliteContext;
        public LoginWindowViewModel(ContextAPIService contextAPIService, ClientSqliteContext clientSqliteContext)
        {
            UserName = "Bruce";
            _contextAPIService = contextAPIService;
            _clientSqliteContext = clientSqliteContext;
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
            var result = await _contextAPIService.PostAsync("user/login", new UserLoginDto
            {
                UserName = UserName,
                Password = password.Password,
                Origin = "win"
            });

            if (result.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<UserLoginResponseDto>(await result.Content.ReadAsStringAsync(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
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
                    Id = data.UserId,
                    Name = data.UserName,
                    RefreshToken = data.RefreshToken,
                    AccessToken = data.AccessToken
                };
                App.ServiceProvider.GetRequiredService<MainWindow>().Show();
                App.ServiceProvider.GetRequiredService<Login>().Close();
            }
            else if (result.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var tips = await result.Content.ReadAsStringAsync();
                Growl.WarningGlobal(new GrowlInfo()
                {
                    WaitTime = 3,
                    Message = tips,
                    ShowDateTime = false
                });
            }
            else
            {
                Growl.WarningGlobal(new GrowlInfo()
                {
                    WaitTime = 3,
                    Message = "登录失败",
                    ShowDateTime = false
                });
            }
        }
    }
}
