using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using pos.Data;
using pos.Models;
using System.Collections.ObjectModel;


namespace pos.ViewModels
{
    public partial class AddDealModel : ObservableObject
    {
        private readonly DB_Services _dbServices;

        [ObservableProperty]
        public ObservableCollection<CategoryModel> _addProductCategory = new();

        [ObservableProperty]
        private CategoryModel _productCategory = null;

        [ObservableProperty]
        public string _dealName;

        [ObservableProperty]
        public string _dealPrice;

        [ObservableProperty]
        private DealItem _currentDeal;

        public ObservableCollection<DealItem> DealItems { get; set; } = new();

        public AddDealModel(DB_Services dbServices)
        {
            _dbServices = dbServices;
            DealItems = new ObservableCollection<DealItem>();
        }

        public async Task InitializeAsync()
        {

            await _dbServices.initDatabase();
            CurrentDeal= new DealItem();
            await GetCategory();
        }

        public async Task GetCategory()
        {
            try
            {
                var categoryList = await _dbServices.GetCategory();
                if (categoryList == null || categoryList.Count == 0)
                {
                    return;
                }
                if (categoryList != null)
                {
                    AddProductCategory.Clear();
                    foreach (var category in categoryList)
                    {
                        AddProductCategory.Add(new CategoryModel
                        {
                            Id = category.Id,
                            Name = category.Name
                        });
                    }

                    AddProductCategory[0].IsSelected = true;
                    ProductCategory = AddProductCategory[0];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        [RelayCommand]
        public async void AddDealItem()
        {
            try
            {
                if(string.IsNullOrEmpty(CurrentDeal.DealName) || string.IsNullOrEmpty(CurrentDeal.UnitPrice.ToString()) || CurrentDeal.Quantity <= 0)
                {
                    await Shell.Current.DisplayAlert("Error", "Please fill all fields", "OK");
                    return;
                }
                var existingItem = DealItems.FirstOrDefault(x => x.DealName == CurrentDeal.DealName);
                if (existingItem == null)
                {
                    existingItem = new DealItem
                    {
                        TempId = CurrentDeal.TempId,
                        DealName = CurrentDeal.DealName,
                        UnitPrice = CurrentDeal.UnitPrice,
                        Quantity = CurrentDeal.Quantity
                    };
                    DealItems.Add(existingItem);
                    CurrentDeal = new DealItem();
                }
                else
                {
                    if(existingItem.TempId == CurrentDeal.TempId)
                    {
                        existingItem.Quantity += CurrentDeal.Quantity;
                    }
                    else
                    {
                        existingItem.Quantity = CurrentDeal.Quantity;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
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

    }
}
