using pos.ViewModels;

namespace pos.Pages;

public partial class DealCartPage : ContentPage
{
	private readonly DealCartModel _dealCartModel;
	public DealCartPage(DealCartModel dealCartModel)
	{
		InitializeComponent();
		BindingContext = dealCartModel;
		_dealCartModel = dealCartModel;
        Loaded += DealCartPage_Loaded;
    }
    private async void DealCartPage_Loaded(object sender, EventArgs e)
    {
        await (BindingContext as DealCartModel).InitializeAsync();
    }
}