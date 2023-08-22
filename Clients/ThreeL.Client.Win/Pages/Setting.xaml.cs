using System.Windows.Controls;
using ThreeL.Client.Win.ViewModels;

namespace ThreeL.Client.Win.Pages
{
    /// <summary>
    /// Interaction logic for Setting.xaml
    /// </summary>
    public partial class Setting : Page
    {
        public Setting(SettingViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
