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
        private List<Order> _allOrders = new();

        public OrderModel(DB_Services dbServices, IPopupService popupService)
        {
            _dbServices = dbServices;
            _popupService = popupService;
            _selectedDate = DateTime.Today;
        }

        [ObservableProperty]
        private ObservableCollection<Order> _orders = new();

        [ObservableProperty]
        private DateTime _selectedDate;

        [ObservableProperty]
        private decimal _dailyTotal;

        [ObservableProperty]
        private int _dailyOrderCount;

        [ObservableProperty]
        private bool _isDailyView = true;

        [ObservableProperty]
        private bool _isMonthlyView = false;

        async partial void OnSelectedDateChanged(DateTime value)
        {
            await FilterOrders();
        }

        [RelayCommand]
        private async Task ShowDaily()
        {
            IsDailyView = true;
            IsMonthlyView = false;
            await FilterOrders();
        }

        [RelayCommand]
        private async Task ShowMonthly()
        {
            IsDailyView = false;
            IsMonthlyView = true;
            await FilterOrders();
        }

        public async Task InitializeAsync()
        {
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
                    _allOrders = orderList.OrderByDescending(o => o.OrderDate).ToList();
                    await FilterOrders();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
            }
        }

        private async Task FilterOrders()
        {
            List<Order> filtered;
            
            if (IsDailyView)
            {
                filtered = _allOrders.Where(o => o.OrderDate.Date == SelectedDate.Date).ToList();
            }
            else // Monthly View
            {
                filtered = _allOrders.Where(o => o.OrderDate.Month == SelectedDate.Month && o.OrderDate.Year == SelectedDate.Year).ToList();
            }
            
            Orders.Clear();
            foreach (var order in filtered)
            {
                Orders.Add(order);
            }

            DailyTotal = filtered.Sum(o => o.TotalAmount);
            DailyOrderCount = filtered.Count;
            
            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task ShowOrderItems(Order order)
        {
            try
            {
                if (order == null) return;

                var orderItems = await _dbServices.GetOrderItems(order.Id);

                if(orderItems == null || orderItems.Count == 0)
                {
                    await Shell.Current.DisplayAlert("No Items", "No items found for this order.", "OK");
                    return;
                }

                StringBuilder message = new StringBuilder();
                message.AppendLine($"Order #{order.OrderNumber}");
                message.AppendLine($"Date: {order.OrderDate:dd-MMM-yyyy hh:mm tt}");
                message.AppendLine("--------------------------");
                message.AppendLine("Items:");

                foreach (var item in orderItems)
                {
                    message.AppendLine($"- {item.ProductName} (x{item.Quantity}): Rs {item.UnitPrice * item.Quantity:N2}");
                }

                message.AppendLine("--------------------------");
                message.AppendLine($"TOTAL: Rs {order.TotalAmount:N2}");
                message.AppendLine($"PAID:  Rs {order.PaymentAmount:N2}");
                message.AppendLine($"CHANGE: Rs {order.ChangeAmount:N2}");

                await Shell.Current.DisplayAlert($"Order Details", message.ToString(), "Close");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ShowOrderItems: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"Failed to show order items", "OK");
            }
        }
    }
}