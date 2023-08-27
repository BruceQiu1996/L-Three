using Microsoft.Extensions.DependencyInjection;
using ThreeL.Client.Shared.Entities;

namespace ThreeL.Client.Mobile
{
    public partial class App : Application
    {
        internal static UserProfile UserProfile;

        public App()
        {
            InitializeComponent();
            MainPage = MauiProgram.ServiceProvider.GetRequiredService<LoginPage>();
        }
    }
}