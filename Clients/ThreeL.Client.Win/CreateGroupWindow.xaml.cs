using System.Windows;
using System.Windows.Input;
using ThreeL.Client.Win.ViewModels;

namespace ThreeL.Client.Win
{
    /// <summary>
    /// Interaction logic for CreateGroupWindow.xaml
    /// </summary>
    public partial class CreateGroupWindow : Window
    {
        public CreateGroupWindow(CreateGroupWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Label_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
