using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using pos.Data;
using System.Collections.ObjectModel;

namespace pos.Popups;

public partial class OrderItemPopup : Popup
{
	private readonly DB_Services _dbServices;

    private readonly ObservableCollection<OrderItem> _orderItems = new();
    public ObservableCollection<OrderItem> OrderItems => _orderItems;
    public OrderItemPopup(Order order,DB_Services dbService)
	{
        InitializeComponent();
        _dbServices = dbService;
        LoadOrderItems(order.Id);
    }

	private async void LoadOrderItems(long orderId)
    {
        try
        {
            var orderItemList = await _dbServices.GetOrderItems(orderId);
            if (orderItemList != null)
            {
                OrderItems.Clear();
                foreach (var orderItem in orderItemList)
                {
                    OrderItems.Add(orderItem);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}