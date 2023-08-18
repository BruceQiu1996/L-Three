using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using ThreeL.Client.Win.MyControls;
using ThreeL.Client.Win.ViewModels;

namespace ThreeL.Client.Win
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login(LoginWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Border_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }

        private void Label_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Label_MouseLeftButtonDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new HandyControl.Controls.Screenshot().Start();
            
            App.ServiceProvider.GetRequiredService<MyScreenShotWindow>().Show();
        }
    }
}
