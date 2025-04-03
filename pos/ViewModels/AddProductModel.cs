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
        public ObservableCollection<CategoryModel> _addProductCategory= new();

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
        private CategoryModel _productCategory = null;

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
               if(categoryList == null || categoryList.Count == 0)
                {
                    return;
                }
                    if (categoryList != null)
                {
                    Categories.Clear();
                    AddProductCategory.Clear();
                    foreach (var category in categoryList)
                    {
                        Categories.Add(new CategoryModel
                        {
                            Id = category.Id,
                            Name = category.Name
                        });
                        AddProductCategory.Add(new CategoryModel
                        {
                            Id = category.Id,
                            Name = category.Name
                        });
                    }

                    AddProductCategory[0].IsSelected = true;
                    Categories[1].IsSelected = true;

                    SelectedCategory = Categories[1];
                    ProductCategory = AddProductCategory[0];
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
            var ProductCategory = AddProductCategory.FirstOrDefault(c => c.IsSelected);
            if (string.IsNullOrEmpty(CurrentProduct.Name) 
                || string.IsNullOrEmpty(CurrentProduct.Price.ToString()) 
                || string.IsNullOrEmpty(CurrentProduct.Description)
                || ProductCategory == null
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
                CategoryId = ProductCategory.Id
            };
            var result = await _dbServices.AddProduct(newProduct);
            if (result > 0)
            {
                await Shell.Current.DisplayAlert("Success", "Product saved successfully", "OK");
                CurrentProduct = new ProductItem();

                await GetProducts();
                //foreach (var category in AddProductCategory)
                //{
                //    category.IsSelected = false;
                //}
            }
        }
        [RelayCommand]
        private async void ProductsCategory(CategoryModel category)
        {
            if (ProductCategory.Id == category.Id)
            {
                return;
            }
            var currentCategory = AddProductCategory.FirstOrDefault(c => c.IsSelected);
            currentCategory.IsSelected = false;
            var newCategory = AddProductCategory.FirstOrDefault(c => c.Id == category.Id);
            newCategory.IsSelected = true;
            ProductCategory = newCategory;
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