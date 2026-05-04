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
        public string _dealName;

        [ObservableProperty]
        public string _dealPrice;

        [ObservableProperty]
        public string _dealBarcode;

        [ObservableProperty]
        private ImageSource _selectedImageSource;

        [ObservableProperty]
        private DealItem _currentDeal;

        [ObservableProperty]
        private decimal _total;

        [ObservableProperty]
        private ObservableCollection<Deal> _deals = new();

        public ObservableCollection<DealItem> DealItems { get; set; } = new();

        private string _selectedImagePath;

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
            CurrentDeal = new DealItem();
            await GetDeals();
        }

        [RelayCommand]
        public async Task PickImage()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select Deal Image",
                    FileTypes = FilePickerFileType.Images
                });

                if (result != null)
                {
                    _selectedImagePath = result.FullPath;
                    SelectedImageSource = ImageSource.FromFile(_selectedImagePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error picking image: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task PickImageForUpdate(Deal deal)
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select Deal Image",
                    FileTypes = FilePickerFileType.Images
                });

                if (result != null)
                {
                    deal.ImagePath = result.FullPath;
                    // Force UI update if needed, though usually Entry bindings work. 
                    // For Image we might need a property change notification.
                    await _dbServices.UpdateDealById(deal);
                    await GetDeals();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error picking image for update: {ex.Message}");
            }
        }

        [RelayCommand]
        public async void AddDealItem()
        {
            try
            {
                if (string.IsNullOrEmpty(CurrentDeal.DealName) || CurrentDeal.UnitPrice <= 0 || CurrentDeal.Quantity <= 0)
                {
                    await Shell.Current.DisplayAlert("Error", "Please fill all item fields", "OK");
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
                    if (existingItem.TempId == CurrentDeal.TempId)
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
        public async void SaveDeal()
        {
            try
            {
                if (string.IsNullOrEmpty(DealName) || DealItems == null || DealItems.Count == 0)
                {
                    await Shell.Current.DisplayAlert("Error", "Please fill all required fields", "OK");
                    return;
                }

                decimal finalDealAmount = Total;
                if (!string.IsNullOrWhiteSpace(DealPrice) && decimal.TryParse(DealPrice, out decimal parsedPrice) && parsedPrice > 0)
                {
                    finalDealAmount = parsedPrice;
                }

                if (finalDealAmount <= 0)
                {
                    await Shell.Current.DisplayAlert("Error", "Deal price must be greater than 0", "OK");
                    return;
                }

                var deal = new Deal
                {
                    DealName = DealName,
                    OrderDate = DateTime.Now,
                    DealAmount = finalDealAmount,
                    CategoryId = -1, // Default to "All Deals"
                    ImagePath = _selectedImagePath,
                    Barcode = DealBarcode
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
                DealPrice = string.Empty;
                Total = 0;
                _selectedImagePath = null;
                SelectedImageSource = null;
                DealItems.Clear();
                await GetDeals();
            }
            catch (Exception ex)
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
