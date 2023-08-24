using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
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
        public RelayCommand CloseSettingsPageCommand { get; set; }

        private readonly ContextAPIService _contextAPIService;
        private readonly FileHelper _fileHelper;
        private readonly GrowlHelper _growlHelper;
        public SettingViewModel(ContextAPIService contextAPIService, FileHelper fileHelper, GrowlHelper growlHelper)
        {
            _fileHelper = fileHelper;
            _growlHelper = growlHelper;
            _contextAPIService = contextAPIService;
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            UploadAvatarCommandAsync = new AsyncRelayCommand(UploadAvatarAsync);
            CloseSettingsPageCommand = new RelayCommand(CloseSettingsPage);
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

            _growlHelper.Success("上传头像成功");
            avatar.SetValue(ImageSelector.UriPropertyKey, default(Uri));
            avatar.SetValue(ImageSelector.PreviewBrushPropertyKey, default(Brush));
            avatar.SetValue(ImageSelector.HasValuePropertyKey, false);
            avatar.SetCurrentValue(FrameworkElement.ToolTipProperty, default);
            avatar.RaiseEvent(new RoutedEventArgs(ImageSelector.ImageUnselectedEvent, this));
            WeakReferenceMessenger.Default.Send<byte[], string>(avatarBytes, "avatar-updated");
        }

        private void CloseSettingsPage()
        {
            WeakReferenceMessenger.Default.Send<string, string>("chat", "switch-page");
        }
    }
}
