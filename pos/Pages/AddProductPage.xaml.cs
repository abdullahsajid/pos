using pos.ViewModels;

namespace pos.Pages;

public partial class AddProductPage : ContentPage
{
	public AddProductPage(AddProductModel addProductModel)
	{
		InitializeComponent();
		BindingContext = addProductModel;
        Loaded += MainPage_Loaded;
    }
    private async void MainPage_Loaded(object sender, EventArgs e)
    {
        await (BindingContext as AddProductModel).InitializeAsync();
    }
}