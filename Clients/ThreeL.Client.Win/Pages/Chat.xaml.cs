using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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

        private void RB_Emoji_Click(object sender, RoutedEventArgs e)
        {
            this.pop.IsOpen = true;
        }

        private void EmojiTabControlUC_Close(object sender, System.EventArgs e)
        {
            var container = new InlineUIContainer(new Image { Source = EmojiTabControlUC.SelectEmoji.Value, Height = 20, Width = 20 }, 
                rtb.CaretPosition);

            rtb.CaretPosition = container.ElementEnd;
            rtb.Focus();
            pop.IsOpen = false;
        }
    }
}
