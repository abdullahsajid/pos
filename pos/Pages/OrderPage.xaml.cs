using pos.ViewModels;
using pos.Data;
using System.Diagnostics;
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
            await _viewModel.ShowOrderItemsCommand.ExecuteAsync(selectedOrder);
            ((CollectionView)sender).SelectedItem = null;
        }
    }

    private void OnChangeMonthClicked(object sender, EventArgs e)
    {
        // On Windows, focusing the DatePicker will open the picker dialog
        DatePickerField.Focus();
    }
}