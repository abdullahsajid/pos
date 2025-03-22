using CommunityToolkit.Mvvm.ComponentModel;
using pos.Data;
using pos.Models;
using System.Collections.ObjectModel;

namespace pos.ViewModels
{
    public partial class AddProductModel : ObservableObject
    {
        private readonly DB_Services _dbServices;

        [ObservableProperty]
        public ObservableCollection<CategoryModel> _categories = new();

        [ObservableProperty]
        public ObservableCollection<ProductModel> _products = new();
        public AddProductModel(DB_Services dbServices)
        {
            _dbServices = dbServices;
        }

        public async Task InitializeAsync()
        {

            await _dbServices.initDatabase();
            await GetCategory();
            //IsLoading = true;
            await GetProduct();
            //IsLoading = false;
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

    }
}
