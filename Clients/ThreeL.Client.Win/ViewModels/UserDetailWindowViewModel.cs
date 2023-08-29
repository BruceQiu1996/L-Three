using Microsoft.Extensions.DependencyInjection;
using System;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Client.Win.Helpers;

namespace ThreeL.Client.Win.ViewModels
{
    public class UserDetailWindowViewModel : DetailWindowViewModel
    {
        public string Sign { get; set; }
        public bool IsFriend { get; set; }

        public DateTime? FriendCreateTime { get; set; }
        public string FriendCreateTimeText => FriendCreateTime == null ? null : App.ServiceProvider.GetService<DateTimeHelper>().ConvertDateTimeToText(FriendCreateTime.Value);
        public string Role { get; set; }
        public string RemarkName { get; set; }

        public UserDetailWindowViewModel()
        {

        }

        public UserDetailWindowViewModel FromDto(UserRoughlyDto userRoughlyDto)
        {
            Id = userRoughlyDto.Id;
            AvatarId = userRoughlyDto.Avatar;
            Name = userRoughlyDto.UserName;
            Sign = userRoughlyDto.Sign;
            IsFriend = userRoughlyDto.IsFriend;
            CreateTime = userRoughlyDto.CreateTime;
            Role = userRoughlyDto.Role;
            RemarkName = userRoughlyDto.RemarkName;
            FriendCreateTime = userRoughlyDto.FriendCreateTime;

            return this;
        }
    }
}
