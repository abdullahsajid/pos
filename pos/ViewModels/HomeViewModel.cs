using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using pos.Models;
using pos.Data;
using System.Collections.ObjectModel;
using MenuItem = pos.Models.ProductModel;
using System.ComponentModel;

namespace pos.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        [ObservableProperty]
        public ObservableCollection<CategoryModel> _categories = new();

        [ObservableProperty]
        public ObservableCollection<ProductModel> _products = new();

        private readonly DB_Services _dbServices;

        [ObservableProperty]    
        private MenuItem[] _menuItems = [];
        public ObservableCollection<CartModel> CartItems { get; set; } = new();

        [ObservableProperty]
        private decimal _total;

        public HomeViewModel(DB_Services dbServices)
        {
            _dbServices = dbServices;
            CartItems = new ObservableCollection<CartModel>();
            CartItems.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (CartModel item in e.NewItems)
                    {
                        item.PropertyChanged += CartItem_PropertyChanged;
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (CartModel item in e.OldItems)
                    {
                        item.PropertyChanged -= CartItem_PropertyChanged;
                    }
                }
                UpdateTotal();
            };
            //Task.Run(async () => await GetCategory());
        }

        private void CartItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CartModel.Quantity) || e.PropertyName == nameof(CartModel.Total))
            {
                UpdateTotal();
            }
        }

        [ObservableProperty]
        private string payment;

        public string Change
        {
            get
            {
                if (decimal.TryParse(Payment, out decimal paymentAmount))
                {
                    decimal changeAmount = paymentAmount - Total;
                    return changeAmount >= 0 ? changeAmount.ToString() : "0";
                }
                return "0";
            }
        }

        partial void OnPaymentChanged(string value)
        {
            OnPropertyChanged(nameof(Change));
        }

        public async Task InitializeAsync()
        {
            await _dbServices.initDatabase();
            await GetCategory();
            await GetProduct();
        }

        public async Task GetCategory()
        {
            try
            {
                var categories = await _dbServices.GetCategory();
                Categories = new ObservableCollection<CategoryModel>(categories);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public async Task GetProduct()
        {
            var products = await _dbServices.GetProducts();
            Products = new ObservableCollection<ProductModel>(products);
        }

        [RelayCommand]
        public void AddToCart(MenuItem menuItem)
        {
            var cartitem = CartItems.FirstOrDefault(c => c.itemId == menuItem.Id);
            if (cartitem == null)
            {
                cartitem = new CartModel
                {
                    itemId = menuItem.Id,
                    Name = menuItem.Name,
                    Price = menuItem.Price,
                    Quantity = 1
                };
                CartItems.Add(cartitem);
                UpdateTotal();
            }
            else
            {
                cartitem.Quantity++;
                UpdateTotal();
            }
        }

        public void UpdateTotal()
        {
            Total = CartItems.Sum(c => c.Total);
        }

        [RelayCommand]
        public void RemoveFromCart(CartModel cartItem)
        {
            CartItems.Remove(cartItem);
            UpdateTotal();
        }

    }
}
