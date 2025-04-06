using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using pos.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;
using pos.Popups;
using CommunityToolkit.Maui.Core.Extensions;

namespace pos.ViewModels
{
    public partial class OrderModel : ObservableObject
    {
        private readonly DB_Services _dbServices;
        private readonly IPopupService popupService;
        public OrderModel(DB_Services db_Services,IPopupService popupService)
        {
            _dbServices = db_Services;
            this.popupService = popupService;
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
                    foreach (var order in orderList)
                    {
                        Orders.Add(order);
                        Debug.WriteLine($"Order Number: {order.OrderNumber}");
                        Debug.WriteLine($"Order Date: {order.OrderDate}");
                        Debug.WriteLine($"Order Total: {order.ChangeAmount}");
                    }
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

                Debug.WriteLine($"ShowOrderItems called for Order ID: {order.Id}");
                Debug.WriteLine("Creating OrderItemsPopup...");
                var popup = new OrderItemPopup(order, _dbServices);
                Debug.WriteLine($"OrderItemsPopup created for Order ID: {popup}");
                Debug.WriteLine("OrderItemsPopup created successfully.");
                Debug.WriteLine("Showing popup...");
                //await this.popupService.ShowPopupAsync(popup);
                this.popupService.ShowPopup(popup);
                Debug.WriteLine("Popup shown successfully.");
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