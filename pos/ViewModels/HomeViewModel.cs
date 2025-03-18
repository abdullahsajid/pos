using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using pos.Models;
using pos.Data;
using System.Collections.ObjectModel;
using MenuItem = pos.Models.ProductModel;

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

        public HomeViewModel(DB_Services dbServices)
        {
            _dbServices = dbServices;
            //Task.Run(async () => await GetCategory());
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
                    quantity = 1
                };
                CartItems.Add(cartitem);
            }
            else
            {
                cartitem.quantity++;
            }
        }

    }
}
