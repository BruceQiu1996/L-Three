using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using ThreeL.Client.Win.Helpers;
using ThreeL.Client.Win.ViewModels.Messages;

namespace ThreeL.Client.Win.ViewModels
{
    public class FriendViewModel : ObservableObject
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Remark { get; set; }
        public string Avatar { get; set; }
        public string DisplayName => string.IsNullOrEmpty(Remark) ? UserName : Remark;

        private ObservableCollection<MessageViewModel> messages;
        public ObservableCollection<MessageViewModel> Messages
        {
            get => messages;
            set => SetProperty(ref messages, value);
        }

        public FriendViewModel()
        {
            Messages = new ObservableCollection<MessageViewModel>
            {
                new TimeMessage()
                {
                    DateTime = App.ServiceProvider.GetService<DateTimeHelper>().ConvertDateTimeToText(DateTime.Now)
                },
                new TextMessage()
                {
                    Text = "你好，我是三L"
                },
                new TextMessage()
                {
                    FromSelf = true,
                    Text = "本效果是基于DotNET framework实现的！\r\n\r\n使用 DotNET framework 创建名为 “圆形头像的动态加载实现” 的WPF模板项目，添加1个Nuget库：MvvmLight 5.4.1.1。"
                },
                new TimeMessage()
                {
                    DateTime = App.ServiceProvider.GetService<DateTimeHelper>().ConvertDateTimeToText(DateTime.Now.AddMinutes(5))
                },
                new ImageMessage()
                {
                    FromSelf = true,
                    ImageLocation = "https://bkimg.cdn.bcebos.com/pic/3b292df5e0fe9925ebbe602c32a85edf8db1711f?x-bce-process=image/watermark,image_d2F0ZXIvYmFpa2U5Mg==,g_7,xp_5,yp_5/format,f_auto"
                }
            };
        }
    }
}
