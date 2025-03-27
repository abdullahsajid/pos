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
        public ObservableCollection<CategoryModel> _categories = new();

        [ObservableProperty]
        public ProductItem[] _products = [];

        [ObservableProperty]
        private MenuItem[] _menuItems = [];

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private ProductModel _menuItem = new();


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
                }
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
