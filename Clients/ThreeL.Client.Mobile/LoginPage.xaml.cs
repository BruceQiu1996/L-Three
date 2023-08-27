using ThreeL.Client.Mobile.ViewModels;

namespace ThreeL.Client.Mobile
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginViewModel loginViewModel)
        {
            InitializeComponent();
            BindingContext = loginViewModel;
        }
    }
}