using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;
using ThreeL.Client.Shared.Configurations;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Client.Shared.Services;
using ThreeL.Client.Win.Helpers;
using ThreeL.Client.Win.Pages;
using ThreeL.Infra.Core.Cryptography;

namespace ThreeL.Client.Win.ViewModels
{
    public class SettingViewModel : ObservableObject
    {
        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public AsyncRelayCommand UploadAvatarCommandAsync { get; set; }

        private readonly ContextAPIService _contextAPIService;
        private readonly FileHelper _fileHelper;
        public SettingViewModel(ContextAPIService contextAPIService, FileHelper fileHelper)
        {
            _fileHelper = fileHelper;
            _contextAPIService = contextAPIService;
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            UploadAvatarCommandAsync = new AsyncRelayCommand(UploadAvatarAsync);
        }

        private async Task LoadAsync()
        {

        }

        private async Task UploadAvatarAsync()
        {
            var avatar = App.ServiceProvider.GetRequiredService<Setting>().avatar;
            if (avatar.Uri == null || !avatar.HasValue || !File.Exists(avatar.Uri.LocalPath))
            {
                return;
            }

            var avatarInfo = new FileInfo(avatar.Uri.LocalPath);
            var data = await File.ReadAllBytesAsync(avatar.Uri.LocalPath);
            string code = data.ToSHA256();
            byte[] avatarBytes = null;
            var resp = await _contextAPIService.GetAsync<UserAvatarCheckExistResponseDto>(string.Format(Const.AVATAR_EXIST, code));
            if (resp.Exist)
            {
                avatarBytes = resp.Avatar;
            }
            else
            {
                avatarBytes =
                        await _contextAPIService.UploadUserAvatarAsync(avatarInfo.Name, data, code);
            }

            WeakReferenceMessenger.Default.Send<byte[], string>(avatarBytes, "avatar-updated");
        }
    }
}
