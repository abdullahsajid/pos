using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using pos.Data;
using pos.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace pos.ViewModels
{
    public partial class AddProductModel : ObservableObject
    {
        private readonly DB_Services _dbServices;

        public AddProductModel(DB_Services dbServices)
        {
            _dbServices = dbServices;
        }

        [ObservableProperty]
        public CategoryModel[] _categories = [];

        [ObservableProperty]
        public ProductItem[] _products = [];

        [ObservableProperty]
        private MenuItem[] _menuItems = [];

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private ProductModel _menuItem = new();

        [ObservableProperty]
        private string newCategoryName;

        public ObservableCollection<MenuCategory> Categoriess { get; set; } = new();

        //[RelayCommand]
        //public void ToggleCategorySelection(MenuCategory category) => category.IsSelected = !category.IsSelected;
        public async Task InitializeAsync()
        {

            await _dbServices.initDatabase();
            await GetCategory();
            IsLoading = true;
            //await GetProduct();
            IsLoading = false;
        }

        public ICommand SaveCategoryCommand { get; }

        public async void SaveCategoryAsync(MenuCategory category)
        {
            if (!string.IsNullOrWhiteSpace(NewCategoryName))
            {
                var newCategory = new MenuCategory
                {
                    Name = category.Name
                };
                Debug.WriteLine($"New Category: {newCategory.Name}");
                int result = await _dbServices.AddCategory(newCategory);
                Debug.WriteLine($"Result: {result}");
                if (result > 0) // If inserted successfully
                {
                    await GetCategory(); // Refresh list
                    NewCategoryName = string.Empty; // Clear input field
                }
            }
        }

        public async Task GetCategory()
        {
            try
            {
                var categories = await _dbServices.GetCategory();
                Debug.WriteLine($"Categories: {categories.Count}");
                //Categories = new ObservableCollection<MenuCategory>(categories);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        //public async Task GetProduct()
        //{
        //    var products = await _dbServices.GetProducts();
        //    Products = new ObservableCollection<ProductItem>(products);
        //}

    }
}

//public void HandleProductItems(ProductModel productModel)
        //{
        //    var product = Products.FirstOrDefault(x => x.Id == productModel.Id);
        //    if(productModel.)
        //}
