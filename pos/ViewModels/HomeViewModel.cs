
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
        public ObservableCollection<MenuItem> _productItems = new();

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
        private decimal _total;

        [ObservableProperty]
        private bool hasProducts;
        [ObservableProperty]
        private bool hasDeals;

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
                var searchResults = await _dbServices.SeachProuctsAsync(SearchText);
                ProductItems = new ObservableCollection<MenuItem>(searchResults);
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

        public string Change
        {
            get
            {
                if (decimal.TryParse(Payment, out decimal paymentAmount))
                {
                    decimal changeAmount = paymentAmount - Total;
                    return changeAmount >= 0 ? changeAmount.ToString() : "0";
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
                if (SelectedCategory == null)
                {
                    HasProducts = false;
                    HasDeals = false;
                    return;
                }
                Debug.WriteLine($"Selected Category: {SelectedCategory.Name}, ID: {SelectedCategory.Id}");
                var productList = await _dbServices.GetProductsByCategory(SelectedCategory.Id);
                Debug.WriteLine($"Products fetched: {productList?.Count ?? 0}");
                //if (SelectedCategory.Name == "Deals")
                //{
                //    var dealList = await _dbServices.GetDealByCategory(SelectedCategory.Id);
                //    Debug.WriteLine($"Deals fetched: {dealList?.Count ?? 0} {dealList?.Count}");
                //    //foreach (var deal in dealList)
                //    //{
                //    //    Deals.Add(deal); 
                //    //}
                //    if (dealList != null && dealList.Any())
                //    {
                //        Debug.WriteLine("Populating Deals...");
                //        foreach (var deal in dealList)
                //        {
                //            if (deal == null)
                //            {
                //                Debug.WriteLine("Found null deal in dealList, skipping...");
                //                continue;
                //            }

                //            try
                //            {
                //                Deals.Add(new Deal
                //                {
                //                    DealName = deal.DealName ?? "Unnamed Deal",
                //                    OrderDate = deal.OrderDate, 
                //                    DealAmount = deal.DealAmount
                //                });
                //                Debug.WriteLine($"Added deal: {deal.DealName}, {deal.DealAmount}");
                //            }
                //            catch (Exception ex)
                //            {
                //                Debug.WriteLine($"Error adding deal: {ex.Message}");
                //            }
                //        }
                //    }
                //    else
                //    {
                //        Debug.WriteLine("dealList is null or empty");
                //    }
                //    //return;
                //}
                //else
                //{
                    if(productList != null)
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
                //}
                HasProducts = Products.Count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
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
        private void AddToCart(MenuItem item)
        {
            try
            {
                CartModel cartitem = null;

                if (item is MenuItem product)
                {
                    cartitem = CartItems.FirstOrDefault(c => c.itemId == product.Id);
                    if (cartitem == null)
                    {
                        cartitem = new CartModel
                        {
                            itemId = product.Id,
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
                UpdateTotal();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error in AddToCart: {e.Message}");
            }
        }

        public void SelectFirstSearchResult()
        {
            if (ProductItems != null && ProductItems.Any())
            {
                AddToCart(ProductItems.First());
            }
            SearchText = string.Empty;
            IsSearchActive = false;
        }

        public void UpdateTotal()
        {
            Total = CartItems.Sum(c => c.Total);
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

            var order = new Order
            {
                OrderNumber = $"ORD-{_lastOrderNumber}",
                OrderDate = DateTime.Now,
                TotalAmount = Total,
                PaymentAmount = decimal.TryParse(Payment, out decimal paymentAmount) ? paymentAmount : 0,
                ChangeAmount = decimal.TryParse(Change, out decimal changeAmount) ? changeAmount : 0
            };

            await _dbServices.AddOrder(order);

            foreach (var item in CartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.itemId,
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

        public async void PrintPageHandler(object sender, System.Drawing.Printing.PrintPageEventArgs e)
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
            g.DrawString($"Invoice: ORD-{_lastOrderNumber}", bodyFont, Brushes.Black, new System.Drawing.PointF(5, yPos));
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
            }

            g.DrawString(new string('-', 45), bodyFont, Brushes.Black, new System.Drawing.PointF(0, yPos));
            yPos += (int)lineSpacing - 5;

            // --- Totals --- (Both Copies for Subtotal)
            g.DrawString("SUBTOTAL:", boldFont, Brushes.Black, new RectangleF(100, yPos, 100, lineSpacing), rightFormat);
            g.DrawString($"{Total:N2}", boldFont, Brushes.Black, new RectangleF(200, yPos, 75, lineSpacing), rightFormat);
            yPos += (int)lineSpacing;

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