
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using pos.Data;
using pos.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using MenuItem = pos.Data.ProductItem;

namespace pos.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        [ObservableProperty]
        public ObservableCollection<CategoryModel> _categories = new();

        [ObservableProperty]
        public ObservableCollection<MenuItem> _products = new();

        [ObservableProperty]
        public ObservableCollection<SearchItemModel> _productItems = new();

        [ObservableProperty]
        public ObservableCollection<Deal> _deals= new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private CategoryModel _selectedCategory = null;

        private readonly DB_Services _dbServices;

        [ObservableProperty]
        private MenuItem[] _menuItems = [];
        public ObservableCollection<CartModel> CartItems { get; set; } = new();

        [ObservableProperty]
        private string _selectedOrderType = "Dine-In";

        [ObservableProperty]
        private decimal _total;

        [ObservableProperty]
        private bool hasProducts;
        [ObservableProperty]
        private bool hasDeals;

        [ObservableProperty]
        private SearchItemModel _selectedSearchResult;

        partial void OnSelectedSearchResultChanged(SearchItemModel value)
        {
            if (ProductItems != null)
            {
                foreach (var item in ProductItems)
                {
                    item.IsSelected = (item == value);
                }
            }
        }

        private string _productSearch;
        private string _lastOrderNumber;
        private Settings? _cachedSettings;

        private string _searchText;
        private bool _isSearchActive;
     
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);

                IsSearchActive = !string.IsNullOrWhiteSpace(value);

                if (IsSearchActive)
                {
                    _ = SearchProducts();
                }
            }
        }

        public string ProductSearch
        {
            get => _productSearch;
            set
            {
                SetProperty(ref _productSearch, value);
                SearchProductItems();
            }
        }


        public bool IsSearchActive
        {
            get => _isSearchActive;
            set => SetProperty(ref _isSearchActive, value);
        }
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
        }

        public async Task SearchProducts()
        {
            try
            {
                var products = await _dbServices.SeachProuctsAsync(SearchText);
                var deals = await _dbServices.SearchDealItemsAsync(SearchText);
                
                var searchResults = new List<SearchItemModel>();
                foreach (var product in products) searchResults.Add(new SearchItemModel(product));
                foreach (var deal in deals) searchResults.Add(new SearchItemModel(deal));

                ProductItems = new ObservableCollection<SearchItemModel>(searchResults);
                if (ProductItems.Any())
                {
                    SelectedSearchResult = ProductItems.First();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SearchProducts: {ex.Message}");
            }
        }

        public async Task SearchProductItems()
        {
            try
            {
                var searchResults = await _dbServices.SeachProuctsAsync(ProductSearch);
                Products = new ObservableCollection<MenuItem>(searchResults);
                
                var dealResults = await _dbServices.SearchDealItemsAsync(ProductSearch);
                Deals = new ObservableCollection<Deal>(dealResults);

                HasProducts = Products.Count > 0;
                HasDeals = Deals.Count > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SearchProductItems: {ex.Message}");
            }
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

        [ObservableProperty]
        private string _discountText = "0";

        partial void OnDiscountTextChanged(string value)
        {
            UpdateTotal();
        }

        public decimal DiscountAmount
        {
            get => decimal.TryParse(DiscountText, out decimal d) ? d : 0;
        }

        public string Change
        {
            get
            {
                if (decimal.TryParse(Payment, out decimal paymentAmount))
                {
                    decimal changeAmount = paymentAmount - Total;
                    return changeAmount >= 0 ? changeAmount.ToString("F2") : "0";
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
            //IsLoading = true;
            await GetProducts();
        }

        public async Task GetCategory()
        {
            try
            {
                var categoryList = await _dbServices.GetCategory();

                if (categoryList != null)
                {
                    Categories.Clear();

                    // Add "All Deals" category at the beginning
                    Categories.Add(new CategoryModel
                    {
                        Id = -1,
                        Name = "All Deals"
                    });

                    foreach (var category in categoryList)
                    {
                        Categories.Add(new CategoryModel
                        {
                            Id = category.Id,
                            Name = category.Name
                        });
                    }
                    Categories[0].IsSelected = true;

                    SelectedCategory = Categories[0];
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
                Products.Clear();
                Deals.Clear();
                HasProducts = false;
                HasDeals = false;

                if (SelectedCategory == null) return;

                if (SelectedCategory.Id == -1) // All Deals
                {
                    var dealList = await _dbServices.GetDeal();
                    if (dealList != null)
                    {
                        foreach (var deal in dealList)
                        {
                            var items = await _dbServices.GetDealItem(deal.Id);
                            if (items != null)
                            {
                                deal.DealItems = items;
                            }
                            Deals.Add(deal);
                        }
                    }
                    HasDeals = Deals.Count > 0;
                }
                else // Normal product category
                {
                    var productList = await _dbServices.GetProductsByCategory(SelectedCategory.Id);
                    if (productList != null)
                    {
                        foreach (var product in productList)
                        {
                            Products.Add(new MenuItem
                            {
                                Id = product.Id,
                                Name = product.Name,
                                Price = product.Price,
                                ImagePath = product.ImagePath
                            });
                        }
                    }
                    HasProducts = Products.Count > 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetProducts: {ex.Message}");
                HasProducts = false;
                HasDeals = false;
            }
        }

        [RelayCommand]
        private async Task SelectCategoryAsync(CategoryModel category)
        {
            if(SelectedCategory.Id == category.Id)
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

        [RelayCommand]
        private async void AddToCart(object item)
        {
            try
            {
                CartModel cartitem = null;

                if (item is MenuItem product)
                {
                    cartitem = CartItems.FirstOrDefault(c => c.ItemId == product.Id);
                    if (cartitem == null)
                    {
                        cartitem = new CartModel
                        {
                            ItemId = product.Id,
                            Name = product.Name,
                            Price = product.Price,
                            Quantity = 1
                        };
                        CartItems.Add(cartitem);
                    }
                    else
                    {
                        cartitem.Quantity++;
                    }
                }
                else if (item is Deal deal)
                {
                    // For deals, we use a unique way to identify them in cart if needed, 
                    // or just add them as separate items. 
                    // Since Deal Id might overlap with Product Id, we might need a way to distinguish.
                    // However, for simplicity now, let's just add it.
                    // To avoid Id conflict in CartItems.FirstOrDefault, we could use a negative Id or a prefix.
                    
                    int dealItemId = (int)(deal.Id + 100000); // Offset deal IDs to avoid conflict with products

                    cartitem = CartItems.FirstOrDefault(c => c.ItemId == dealItemId);
                    if (cartitem == null)
                    {
                        string subItemsText = "";
                        if (deal.DealItems == null || deal.DealItems.Count == 0)
                        {
                            var items = await _dbServices.GetDealItem(deal.Id);
                            if (items != null)
                            {
                                deal.DealItems = items;
                            }
                        }

                        if (deal.DealItems != null && deal.DealItems.Count > 0)
                        {
                            subItemsText = string.Join(", ", deal.DealItems.Select(di => $"{di.Quantity}x {di.DealName}"));
                        }

                        cartitem = new CartModel
                        {
                            ItemId = dealItemId,
                            Name = deal.DealName,
                            Price = deal.DealAmount,
                            Quantity = 1,
                            SubItems = subItemsText
                        };
                        CartItems.Add(cartitem);
                    }
                    else
                    {
                        cartitem.Quantity++;
                    }
                }
                UpdateTotal();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error in AddToCart: {e.Message}");
            }
        }

        public void SelectFirstSearchResult()
        {
            if (SelectedSearchResult != null)
            {
                AddToCart(SelectedSearchResult.Item);
            }
            SearchText = string.Empty;
            IsSearchActive = false;
            SelectedSearchResult = null;
        }

        public void UpdateTotal()
        {
            var subTotal = CartItems.Sum(c => c.Total);
            Total = subTotal - DiscountAmount;
            if (Total < 0) Total = 0;
            OnPropertyChanged(nameof(Change));
        }

        [RelayCommand]
        public void RemoveFromCart(CartModel cartItem)
        {
            CartItems.Remove(cartItem);
            UpdateTotal();
        }

        [RelayCommand]
        public void FocusSearch()
        {
            MessagingCenter.Send(this, "FocusCartSearch");
        }

        [RelayCommand]
        private void SelectOrderType(string type)
        {
            SelectedOrderType = type;
        }

        [RelayCommand]
        public async void PrintInvoice()
        {
            Debug.WriteLine("Printing Invoice");
            if (CartItems.Count == 0)
            {
                await Shell.Current.DisplayAlert("Error", "Please add CartItems!", "OK");
                return;
            }
            _cachedSettings = await _dbServices.getSettings();
            _lastOrderNumber = await _dbServices.GetNextOrderNumber();

            var subTotal = CartItems.Sum(c => c.Total);
            var order = new Order
            {
                OrderNumber = $"ORD-{_lastOrderNumber}",
                OrderDate = DateTime.Now,
                SubTotal = subTotal,
                Discount = DiscountAmount,
                TotalAmount = Total,
                PaymentAmount = decimal.TryParse(Payment, out decimal paymentAmount) ? paymentAmount : 0,
                ChangeAmount = decimal.TryParse(Change, out decimal changeAmount) ? changeAmount : 0,
                OrderType = SelectedOrderType
            };

            await _dbServices.AddOrder(order);

            foreach (var item in CartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ItemId,
                    Quantity = item.Quantity,
                    ProductName = item.Name,
                    CreatedDate = DateTime.Now,
                    UnitPrice = item.Price
                };
                await _dbServices.AddOrderItem(orderItem);
            }

            _currentPage = 0;
            _totalPages = 2;

            PrintDocument printDoc = new PrintDocument();

            printDoc.PrintPage += PrintPageHandler;
            printDoc.Print();
            Debug.WriteLine("Invoice Printed");
        }

        private int _currentPage = 0;
        private int _totalPages = 2;

        public void PrintPageHandler(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            int yPos = 20;
            float lineSpacing = 22; // Tighter line spacing for receipt
            var settings = _cachedSettings ?? new Settings { CompanyName = "POS System", CompanyAddress = "Address", CompanyPhone = "000-0000000" };

            // For 80mm thermal at 203 DPI, usable width is approx 280-300px
            float pageWidth = 280; // Standard usable width for 80mm thermal

            StringFormat centerFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
            };

            StringFormat rightFormat = new StringFormat
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center,
            };

            StringFormat leftFormat = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center,
            };

            // Font Families
            var headerFont = new System.Drawing.Font("Arial", 14, FontStyle.Bold);
            var subHeaderFont = new System.Drawing.Font("Arial", 9);
            var bodyFont = new System.Drawing.Font("Arial", 9);
            var boldFont = new System.Drawing.Font("Arial", 9, FontStyle.Bold);

            // --- Header --- (Customer Copy Only)
            if (_currentPage == 1)
            {
                // Draw Logo if available
                if (!string.IsNullOrEmpty(settings.Image) && System.IO.File.Exists(settings.Image))
                {
                    try
                    {
                        using (System.Drawing.Image logo = System.Drawing.Image.FromFile(settings.Image))
                        {
                            // Calculate logo dimensions: Fixed width of 100px, maintain aspect ratio
                            int targetWidth = 100;
                            int targetHeight = (int)((float)logo.Height * targetWidth / logo.Width);
                            
                            // Center horizontally
                            int xPos = (int)((pageWidth - targetWidth) / 2);
                            
                            g.DrawImage(logo, new Rectangle(xPos, yPos, targetWidth, targetHeight));
                            yPos += targetHeight + 10;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to print logo: {ex.Message}");
                    }
                }

                g.DrawString(settings.CompanyName, headerFont, Brushes.Black,
                    new RectangleF(0, yPos, pageWidth, lineSpacing + 5), centerFormat);
                yPos += (int)lineSpacing + 8;

                g.DrawString(settings.CompanyAddress, subHeaderFont, Brushes.Black,
                    new RectangleF(0, yPos, pageWidth, lineSpacing), centerFormat);
                yPos += (int)lineSpacing;

                g.DrawString(settings.CompanyPhone, subHeaderFont, Brushes.Black,
                    new RectangleF(0, yPos, pageWidth, lineSpacing), centerFormat);
                yPos += (int)lineSpacing;

                g.DrawString(new string('-', 45), bodyFont, Brushes.Black, new System.Drawing.PointF(0, yPos));
                yPos += (int)lineSpacing - 5;
            }

            // --- Order Info --- (Both Copies)
            g.DrawString($"Invoice: ORD-{_lastOrderNumber} ({SelectedOrderType})", bodyFont, Brushes.Black, new System.Drawing.PointF(5, yPos));
            yPos += (int)lineSpacing;

            g.DrawString($"Date: {DateTime.Now:dd-MM-yy hh:mm tt}", bodyFont, Brushes.Black, new System.Drawing.PointF(5, yPos));
            yPos += (int)lineSpacing;

            string copyType = _currentPage == 0 ? "KITCHEN COPY" : "CUSTOMER COPY";
            g.DrawString($"*** {copyType} ***", boldFont, Brushes.Black, new RectangleF(0, yPos, pageWidth, lineSpacing), centerFormat);
            yPos += (int)lineSpacing;

            g.DrawString(new string('-', 45), bodyFont, Brushes.Black, new System.Drawing.PointF(0, yPos));
            yPos += (int)lineSpacing - 5;

            // --- Items Header --- (Both Copies)
            g.DrawString("ITEM", boldFont, Brushes.Black, new RectangleF(5, yPos, 150, lineSpacing), leftFormat);
            g.DrawString("QTY", boldFont, Brushes.Black, new RectangleF(155, yPos, 40, lineSpacing), rightFormat);
            g.DrawString("TOTAL", boldFont, Brushes.Black, new RectangleF(200, yPos, 75, lineSpacing), rightFormat);
            yPos += (int)lineSpacing;

            g.DrawString(new string('-', 45), bodyFont, Brushes.Black, new System.Drawing.PointF(0, yPos));
            yPos += (int)lineSpacing - 5;

            // --- Items --- (Both Copies)
            foreach (var item in CartItems)
            {
                // Item Name (can wrap if needed, but for now single line)
                string itemName = item.Name;
                if (itemName.Length > 50) itemName = itemName.Substring(0, 50) + "...";

                g.DrawString(itemName, bodyFont, Brushes.Black, new RectangleF(5, yPos, 150, lineSpacing), leftFormat);
                g.DrawString(item.Quantity.ToString(), bodyFont, Brushes.Black, new RectangleF(155, yPos, 40, lineSpacing), rightFormat);
                g.DrawString($"{item.Price * item.Quantity:N2}", bodyFont, Brushes.Black, new RectangleF(200, yPos, 75, lineSpacing), rightFormat);
                yPos += (int)lineSpacing;

                if (!string.IsNullOrEmpty(item.SubItems))
                {
                    // Print sub-items in a smaller font, indented
                    var subFont = new System.Drawing.Font("Arial", 8, FontStyle.Italic);
                    string[] subLines = item.SubItems.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in subLines)
                    {
                        g.DrawString($"  - {line}", subFont, Brushes.Black, new RectangleF(5, yPos, 200, lineSpacing - 4), leftFormat);
                        yPos += (int)(lineSpacing - 4);
                    }
                    yPos += 2; // small padding after sub-items
                }
            }

            g.DrawString(new string('-', 45), bodyFont, Brushes.Black, new System.Drawing.PointF(0, yPos));
            yPos += (int)lineSpacing - 5;

            // --- Totals --- (Both Copies)
            var receiptSubTotal = CartItems.Sum(c => c.Total);
            g.DrawString("SUBTOTAL:", boldFont, Brushes.Black, new RectangleF(100, yPos, 100, lineSpacing), rightFormat);
            g.DrawString($"{receiptSubTotal:N2}", boldFont, Brushes.Black, new RectangleF(200, yPos, 75, lineSpacing), rightFormat);
            yPos += (int)lineSpacing;

            if (DiscountAmount > 0)
            {
                g.DrawString("DISCOUNT:", bodyFont, Brushes.Black, new RectangleF(100, yPos, 100, lineSpacing), rightFormat);
                g.DrawString($"- {DiscountAmount:N2}", bodyFont, Brushes.DarkRed, new RectangleF(200, yPos, 75, lineSpacing), rightFormat);
                yPos += (int)lineSpacing;

                g.DrawString("TOTAL:", boldFont, Brushes.Black, new RectangleF(100, yPos, 100, lineSpacing), rightFormat);
                g.DrawString($"{Total:N2}", boldFont, Brushes.Black, new RectangleF(200, yPos, 75, lineSpacing), rightFormat);
                yPos += (int)lineSpacing;
            }

            // --- Payment & Footer --- (Customer Copy Only)
            if (_currentPage == 1)
            {
                g.DrawString("PAYMENT:", bodyFont, Brushes.Black, new RectangleF(100, yPos, 100, lineSpacing), rightFormat);
                g.DrawString($"{(decimal.TryParse(Payment, out decimal p) ? p : 0):N2}", bodyFont, Brushes.Black, new RectangleF(200, yPos, 75, lineSpacing), rightFormat);
                yPos += (int)lineSpacing;

                g.DrawString("CHANGE:", bodyFont, Brushes.Black, new RectangleF(100, yPos, 100, lineSpacing), rightFormat);
                g.DrawString($"{(decimal.TryParse(Change, out decimal c) ? c : 0):N2}", bodyFont, Brushes.Black, new RectangleF(200, yPos, 75, lineSpacing), rightFormat);
                yPos += (int)lineSpacing + 5;

                g.DrawString(new string('=', 45), bodyFont, Brushes.Black, new System.Drawing.PointF(0, yPos));
                yPos += (int)lineSpacing - 5;

                // --- Footer ---
                g.DrawString("THANK YOU!", boldFont, Brushes.Black,
                    new RectangleF(0, yPos, pageWidth, lineSpacing), centerFormat);
                yPos += (int)lineSpacing;

                g.DrawString("*** ENJOY YOUR MEAL ***", subHeaderFont, Brushes.Black,
                    new RectangleF(0, yPos, pageWidth, lineSpacing), centerFormat);
            }

            _currentPage++;

            e.HasMorePages = _currentPage < _totalPages;

            if (!e.HasMorePages)
            {
                CartItems.Clear();
                Total = 0;
                Payment = string.Empty;
            }
        }

    }
}