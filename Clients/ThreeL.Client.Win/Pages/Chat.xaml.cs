using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ThreeL.Client.Win.ViewModels;

namespace ThreeL.Client.Win.Pages
{
    /// <summary>
    /// Interaction logic for Friend.xaml
    /// </summary>
    public partial class Chat : Page
    {
        public Chat(ChatViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void ListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

        private void ScrollViewer_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var window = App.ServiceProvider.GetRequiredService<CreateGroupWindow>();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Owner = App.ServiceProvider.GetRequiredService<MainWindow>();
            window.ShowDialog();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            toSendText.Focus();
        }
    }
}
