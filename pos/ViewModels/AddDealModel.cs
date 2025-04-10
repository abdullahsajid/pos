using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using pos.Data;
using pos.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;


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

        [ObservableProperty]
        private decimal _total;

        [ObservableProperty]
        private ObservableCollection<Deal> _deals = new();

        public ObservableCollection<DealItem> DealItems { get; set; } = new();

        public AddDealModel(DB_Services dbServices)
        {
            _dbServices = dbServices;
            DealItems = new ObservableCollection<DealItem>();
            DealItems.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (DealItem item in e.NewItems)
                    {
                        item.PropertyChanged += CartItem_PropertyChanged;
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (DealItem item in e.OldItems)
                    {
                        item.PropertyChanged -= CartItem_PropertyChanged;
                    }
                }
                UpdateTotal();
            };
        }

        private void CartItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DealItem.Quantity) || e.PropertyName == nameof(DealItem.UnitPrice))
            {
                UpdateTotal();
            }
        }

        public async Task InitializeAsync()
        {

            await _dbServices.initDatabase();
            CurrentDeal= new DealItem();
            await GetCategory();
            await GetDeals();
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
                    UpdateTotal();
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

        public void UpdateTotal() 
        {
            Total = DealItems.Sum(x => x.SubTotal);
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
        public async void SaveDeal()
        {
            try
            {
                var ProductCategory = AddProductCategory.FirstOrDefault(c => c.IsSelected);
                if (string.IsNullOrEmpty(DealName) || Total <= 0 || DealItems == null || DealItems.Count == 0)
                {
                    await Shell.Current.DisplayAlert("Error", "Please fill all fields", "OK");
                    return;
                }
                var deal = new Deal
                {
                    DealName = DealName,
                    OrderDate = DateTime.Now,
                    DealAmount = Total,
                    CategoryId = ProductCategory.Id
                };
                await _dbServices.AddDeal(deal);

                foreach (var item in DealItems)
                {
                    var dealItem = new DealItem
                    {
                        DealId = deal.Id,
                        DealName = item.DealName,
                        CreatedDate = DateTime.Now,
                        UnitPrice = item.UnitPrice,
                        Quantity = item.Quantity,
                        SubTotal = item.SubTotal
                    };
                    await _dbServices.AddDealItem(dealItem);
                }
                DealName = string.Empty;
                Total = 0;
                DealItems.Clear();
                await GetDeals();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }


        public async Task GetDeals()
        {
            try
            {
                var dealList = await _dbServices.GetDeal();
                if (dealList != null)
                {
                    Deals.Clear();
                    Deals = new ObservableCollection<Deal>(dealList);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        [RelayCommand]
        public async void UpdateDeal(Deal deal)
        {
            try
            {
                var result = await _dbServices.UpdateDealById(deal);
                Debug.WriteLine("Result: " + result);
                if (result > 0)
                {
                    await GetDeals();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                await Shell.Current.DisplayAlert("Error", "Something went wrong!", "OK");
            }
        }

        [RelayCommand]
        public async void DeleteDeal(Deal deal)
        {
            try
            {
                var result = await _dbServices.DeleteDealById(deal);
                Debug.WriteLine("Result: " + result);
                if (result > 0)
                {
                    await GetDeals();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                await Shell.Current.DisplayAlert("Error", "Something went wrong!", "OK");
            }
        }
    }
}
