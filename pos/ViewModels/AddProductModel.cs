using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using pos.Data;
using pos.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Products = pos.Data.ProductItem;

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
        public ObservableCollection<CategoryModel> _categories = new();

        [ObservableProperty]
        public ObservableCollection<ProductModel> _products = new();

        [ObservableProperty]
        private MenuItem[] _menuItems = [];

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private ProductModel _menuItem = new();

        [ObservableProperty]
        private CategoryModel _selectedCategory = null;

        [ObservableProperty]
        private ProductItem _currentProduct;
        public async Task InitializeAsync()
        {

            await _dbServices.initDatabase();
            CurrentProduct = new ProductItem();
            await GetCategory();
            IsLoading = true;
            await GetProducts();
            IsLoading = false;
        }

        public async Task GetCategory()
        {
            try
            {
               var categoryList = await _dbServices.GetCategory();
               Debug.WriteLine($"CategoryList: {categoryList.Count}");
                if (categoryList != null)
                {
                    Categories.Clear();
                    foreach (var category in categoryList)
                    {
                        Categories.Add(new CategoryModel
                        {
                            Id = category.Id,
                            Name = category.Name
                        });
                    }
                    Categories[1].IsSelected = true;

                    SelectedCategory = Categories[1];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public async Task GetProducts()
        {
            try
            {
                var productList = await _dbServices.GetProductsByCategory(SelectedCategory.Id);
                
                if (productList != null)
                {
                    Products.Clear();
                    foreach (var product in productList)
                    {
                        Products.Add(new ProductModel
                        {
                            Id = product.Id,
                            Name = product.Name,
                            Price = product.Price
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        [RelayCommand]
        private async void SaveProduct()
        {
            var selectedCategory = Categories.FirstOrDefault(c => c.IsSelected);
            if (string.IsNullOrEmpty(CurrentProduct.Name) 
                || string.IsNullOrEmpty(CurrentProduct.Price.ToString()) 
                || string.IsNullOrEmpty(CurrentProduct.Description)
                || selectedCategory == null
            )
            {
                await Shell.Current.DisplayAlert("Error", "Please fill all fields", "OK");
                return;
            }
            var newProduct = new ProductItem
            {
                Name = CurrentProduct.Name,
                Price = CurrentProduct.Price,
                Description = CurrentProduct.Description,
                CategoryId = selectedCategory.Id
            };
            var result = await _dbServices.AddProduct(newProduct);
            if (result > 0)
            {
                await Shell.Current.DisplayAlert("Success", "Product saved successfully", "OK");
                CurrentProduct = new ProductItem();

                await GetProducts();
                foreach (var category in Categories)
                {
                    category.IsSelected = false;
                }
            }
        }

        [RelayCommand]
        private async Task SelectCategoryAsync(CategoryModel category)
        {
            if (SelectedCategory.Id == category.Id)
            {
                return;
            }

            var currentCategory = Categories.FirstOrDefault(c => c.IsSelected);
            currentCategory.IsSelected = false;

            var newCategory = Categories.FirstOrDefault(c => c.Id == category.Id);
            newCategory.IsSelected = true;

            SelectedCategory = newCategory;

            await GetProducts();

        }

        //public async Task GetProduct()
        //{
        //    var products = await _dbServices.GetProducts();
        //    Products = new ObservableCollection<ProductItem>(products);
        //}

    }
}