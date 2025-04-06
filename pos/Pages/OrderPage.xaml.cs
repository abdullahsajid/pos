using pos.ViewModels;
using pos.Data;
namespace pos.Pages;

public partial class OrderPage : ContentPage
{
    private readonly OrderModel _viewModel;
    public OrderPage(OrderModel orderModel)
	{
		InitializeComponent();
        BindingContext = orderModel;
        _viewModel = orderModel;
        Loaded += MainPage_Loaded;
    }
    private async void MainPage_Loaded(object sender, EventArgs e)
    {
        await (BindingContext as OrderModel).InitializeAsync();
    }
    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Order selectedOrder)
        {
            // Call the ShowOrderItemsCommand from the view model
            await _viewModel.ShowOrderItemsCommand.ExecuteAsync(selectedOrder);
        }
    }
}