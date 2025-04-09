using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using pos.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace pos.ViewModels
{
    public partial class OrderModel : ObservableObject
    {
        private readonly DB_Services _dbServices;
        private readonly IPopupService _popupService;
        public OrderModel(DB_Services dbServices, IPopupService popupService)
        {
            _dbServices = dbServices;
            _popupService = popupService;
        }

        [ObservableProperty]
        private ObservableCollection<Order> _orders = new();

        public async Task InitializeAsync()
        {
            Debug.WriteLine("InitializeAsync called");
            await _dbServices.initDatabase();
            await GetOrders();
        }
        public async Task GetOrders()
        {
            try
            {
                var orderList = await _dbServices.GetOrder();
                if (orderList != null)
                {
                    Orders.Clear();
                    Orders = new ObservableCollection<Order>(orderList);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ShowOrderItems(Order order)
        {
            try
            {
                if (order == null)
                {
                    Debug.WriteLine("ShowOrderItems: Selected order is null.");
                    return;
                }

                var orderItems = await _dbServices.GetOrderItems(order.Id);

                if(orderItems == null || orderItems.Count == 0)
                {
                    await Shell.Current.DisplayAlert("No Items", "No items found for this order.", "OK");
                    return;
                }

                StringBuilder message = new StringBuilder();
                message.AppendLine($"Order #{order.Id}");
                message.AppendLine($"Date: {order.OrderDate}");
                message.AppendLine();
                message.AppendLine("Items:");

                decimal total = 0;
                foreach (var item in orderItems)
                {
                    message.AppendLine($"- {item.ProductName} (x{item.Quantity}): {item.UnitPrice * item.Quantity:C}");
                    total += item.UnitPrice * item.Quantity;
                }

                message.AppendLine();
                message.AppendLine($"Total: {total:C}");

                await Shell.Current.DisplayAlert($"Order Details", message.ToString(), "Close");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ShowOrderItems: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error", $"Failed to show order items: {ex.Message}", "OK");
            }
        }
    }
}