using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using ThreeL.Client.Win.ViewModels;

namespace ThreeL.Client.Win
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SetWindowDisplayAffinity(IntPtr hwnd, uint mode);

        public MainWindow(MainWindowViewModel viewModel)
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

        private async void Label_MouseLeftButtonDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            await App.CloseAsync();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetWindowDisplayAffinity(new WindowInteropHelper(this).Handle, 1);
        }
    }
}
