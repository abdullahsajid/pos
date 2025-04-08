namespace pos.Pages;
using pos.Data;
using pos.ViewModels;
public partial class AddDealPage : ContentPage
{
	private readonly DB_Services _dbServices;
    public AddDealPage(AddDealModel addDealModel,DB_Services dbServices)
	{
		InitializeComponent();
		_dbServices = dbServices;
        BindingContext = addDealModel;
        Loaded += MainPage_Loaded;
    }

    private async void MainPage_Loaded(object sender, EventArgs e)
    {
        await (BindingContext as AddDealModel).InitializeAsync();
    }
}