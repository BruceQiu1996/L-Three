using ThreeL.Client.Mobile.ViewModels;

namespace ThreeL.Client.Mobile.Pages;

public partial class MainPage : TabbedPage
{
	public MainPage(MainPageViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}