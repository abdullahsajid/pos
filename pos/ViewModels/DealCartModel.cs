using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using pos.Data;
using pos.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Text.Json;

namespace pos.ViewModels
{
    public partial class DealCartModel : ObservableObject
    {
        [ObservableProperty]
        public ObservableCollection<CategoryModel> _categories = new();

        [ObservableProperty]
        public ObservableCollection<ProductItem> _productItem = new();

        [ObservableProperty]
        public ObservableCollection<ProductItem> _dealsItems = new();

        [ObservableProperty]
        public ObservableCollection<SearchResult> _searchProductsItems = new();

        private readonly DB_Services _dbServices;

        [ObservableProperty]
        private CategoryModel _selectedCategory = null;

        [ObservableProperty]
        private decimal _total;

        [ObservableProperty]
        private string address;

        [ObservableProperty]
        private string phone;

        [ObservableProperty]
        private bool hasProducts;
        [ObservableProperty]
        private bool hasDeals;

        private string _searchText;
        private bool _isSearchActive;
        private string _productSearch;
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);

                IsSearchActive = !string.IsNullOrWhiteSpace(value);

                if (IsSearchActive)
                {
                    _ = SearchDeals();
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

        public async Task SearchDeals()
        {
            try
            {
                DealsItems.Clear();
                var searchResults = await _dbServices.SearchDealItemsAsync(SearchText);
                if(searchResults.Count > 0)
                {
                    foreach (var deal in searchResults)
                    {
                        DealsItems.Add(new ProductItem
                        {
                            Id = (int)deal.Id,
                            Name = deal.DealName,
                            Price = deal.DealAmount
                        });
                    }
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
                ProductItem.Clear();
                SearchProductsItems.Clear();
                if (string.IsNullOrWhiteSpace(ProductSearch))
                {
                    HasProducts = false;
                    HasDeals = false;
                    return;
                }
                var searchResults = await _dbServices.SearchProductDealAsync(ProductSearch);
                var json = JsonSerializer.Serialize(searchResults, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                Debug.WriteLine($"Search Results (JSON): {json}");
                foreach (var deal in searchResults)
                {
                    SearchProductsItems.Add(new SearchResult
                    {
                        Id = (int)deal.Id,
                        Name = deal.Name,
                        Price = deal.Price
                    });
                }
                HasProducts = SearchProductsItems.Count > 0;
                HasDeals = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SearchProductItems: {ex.Message}");
                HasProducts = false;
            }
        }

        public ObservableCollection<CartModel> CartItems { get; set; } = new();
        public DealCartModel(DB_Services dbServices)
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
        private void CartItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CartModel.Quantity) || e.PropertyName == nameof(CartModel.Total))
            {
                UpdateTotal();
            }
        }

        public async Task InitializeAsync()
        {
            await _dbServices.initDatabase();
            await GetCategory();
            await GetProducts();
            await GetDeals();
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

        public async Task GetCategory()
        {
            try
            {
                var categoryList = await _dbServices.GetCategory();
                Debug.WriteLine($"Category List Count: {categoryList.Count}");
                if (categoryList != null)
                {
                    Categories.Clear();
                    Categories.Add(new CategoryModel
                    {
                        Id = 0,
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

        public async Task GetDeals()
        {
            try
            {
                ProductItem.Clear();
                SearchProductsItems.Clear();
                if (SelectedCategory.Id == 0)
                {
                    var dealList = await _dbServices.GetDeal();
                    foreach (var deal in dealList)
                    {
                        ProductItem.Add(new ProductItem
                        {
                            Id = (int)deal.Id,
                            Name = deal.DealName,
                            Price = deal.DealAmount
                        });
                    }
                    HasDeals = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                HasDeals = false;
            }
        }

        public async Task GetProducts()
        {
            try
            {
                ProductItem.Clear();
                SearchProductsItems.Clear();
                var productList = await _dbServices.GetProductsByCategory(SelectedCategory.Id);
                
                if (productList != null)
                {
                    foreach (var product in productList)
                    {
                        ProductItem.Add(new ProductItem
                        {
                            Id = product.Id,
                            Name = product.Name,
                            Price = product.Price
                        });
                    }
                    HasDeals = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                HasDeals = false;
            }
        }

        [RelayCommand]
        private void AddToCart(ProductItem item)
        {
            try
            {
                CartModel cartitem = null;
                cartitem = CartItems.FirstOrDefault(c => c.itemId == item.Id);
                if (cartitem == null)
                {
                     cartitem = new CartModel
                     {
                         itemId = item.Id,
                         Name = item.Name,
                         Price = item.Price,
                         Quantity = 1
                     };
                     CartItems.Add(cartitem);
                    Debug.WriteLine($"Added to cart: {cartitem.Name} with quantity {cartitem.Quantity}");
                }
                else
                {
                   cartitem.Quantity++;
                }
                UpdateTotal();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error in AddToCart: {e.Message}");
            }
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
            if(SelectedCategory.Id == 0)
            {
                await GetDeals();
            }
        }

        [RelayCommand]
        public async void PrintInvoice()
        {
            Debug.WriteLine("Printing Invoice");
            var orderNumber = await _dbServices.GetNextOrderNumber();

            var order = new Order
            {
                OrderNumber = $"ORD-{orderNumber}",
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

        public void PrintPageHandler(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            int yPos = 30;
            float lineSpacing = 30;

            StringFormat centerFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
            };

            float pageWidth = e.PageBounds.Width;

            string copyType = _currentPage == 0 ? "Merchant Copy" : "Customer Copy";

            g.DrawString("Super Sweets Bakers", new System.Drawing.Font("Arial", 20, FontStyle.Bold), Brushes.Black, new RectangleF(0, yPos, pageWidth, lineSpacing), centerFormat);
            yPos += (int)lineSpacing;

            g.DrawString("Laiq Ali Chowk Wah Cantt", new System.Drawing.Font("Arial", 17), Brushes.Black, new RectangleF(0, yPos, pageWidth, lineSpacing), centerFormat);
            yPos += (int)lineSpacing;

            g.DrawString("Phone no# 03135172181", new System.Drawing.Font("Arial", 15), Brushes.Black, new RectangleF(0, yPos, pageWidth, lineSpacing), centerFormat);
            yPos += (int)lineSpacing;

            g.DrawString($"Customer:", new System.Drawing.Font("Arial", 12, FontStyle.Bold), Brushes.Black, new System.Drawing.PointF(10, yPos));
            yPos += (int)lineSpacing;

            string dateLabel = "Date: ";
            float dateLabelWidth = g.MeasureString(dateLabel, new System.Drawing.Font("Arial", 8, FontStyle.Bold)).Width;
            g.DrawString(dateLabel, new System.Drawing.Font("Arial", 12, FontStyle.Bold), Brushes.Black, new System.Drawing.PointF(10, yPos));

            string dateValue = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            float spaceBetween = 20;
            g.DrawString(dateValue, new System.Drawing.Font("Arial", 12), Brushes.Black, new System.Drawing.PointF(10 + dateLabelWidth + spaceBetween, yPos));
            yPos += (int)lineSpacing;

            if(copyType == "Merchant Copy")
            {
                string addressLabel = "Customer Address: ";
                float addressLabelWidth = g.MeasureString(addressLabel, new System.Drawing.Font("Arial", 8, FontStyle.Bold)).Width;
                g.DrawString(addressLabel, new System.Drawing.Font("Arial", 12, FontStyle.Bold), Brushes.Black, new System.Drawing.PointF(10, yPos));

                string addressValue = Address;
                float spaceBetweenAdress = 50;
                g.DrawString(addressValue, new System.Drawing.Font("Arial", 12), Brushes.Black, new System.Drawing.PointF(10 + addressLabelWidth + spaceBetweenAdress, yPos));
                yPos += (int)lineSpacing;

                string phoneLabel = "Customer Phone No: ";
                float phoneLabelWidth = g.MeasureString(phoneLabel, new System.Drawing.Font("Arial", 8, FontStyle.Bold)).Width;
                g.DrawString(phoneLabel, new System.Drawing.Font("Arial", 12, FontStyle.Bold), Brushes.Black, new System.Drawing.PointF(10, yPos));

                string phoneValue = Phone;
                float spaceBetweenPhone = 50;
                g.DrawString(phoneValue, new System.Drawing.Font("Arial", 12), Brushes.Black, new System.Drawing.PointF(10 + phoneLabelWidth + spaceBetweenPhone, yPos));
                yPos += (int)lineSpacing;
            }

            g.DrawString("Product", new System.Drawing.Font("Arial", 12, FontStyle.Bold), Brushes.Black, new System.Drawing.PointF(10, yPos));
            g.DrawString("Qty", new System.Drawing.Font("Arial", 12, FontStyle.Bold), Brushes.Black, new System.Drawing.PointF(170, yPos));
            g.DrawString("Price", new System.Drawing.Font("Arial", 12, FontStyle.Bold), Brushes.Black, new System.Drawing.PointF(220, yPos));
            g.DrawString("T-Amount", new System.Drawing.Font("Arial", 12, FontStyle.Bold), Brushes.Black, new System.Drawing.PointF(270, yPos));

            yPos += (int)lineSpacing;
            foreach (var item in CartItems)
            {
                g.DrawString(item.Name, new System.Drawing.Font("Arial", 12), Brushes.Black, new System.Drawing.PointF(10, yPos));
                g.DrawString(item.Quantity.ToString(), new System.Drawing.Font("Arial", 12), Brushes.Black, new System.Drawing.PointF(170, yPos));
                g.DrawString(item.Price.ToString(), new System.Drawing.Font("Arial", 12), Brushes.Black, new System.Drawing.PointF(220, yPos));
                g.DrawString(item.Total.ToString(), new System.Drawing.Font("Arial", 12), Brushes.Black, new System.Drawing.PointF(270, yPos));
                yPos += (int)lineSpacing;
            }
            yPos += (int)lineSpacing;
            g.DrawString("Total Amount: ", new System.Drawing.Font("Arial", 12, FontStyle.Bold), Brushes.Black, new System.Drawing.PointF(10, yPos));
            g.DrawString(Total.ToString(), new System.Drawing.Font("Arial", 12, FontStyle.Bold), Brushes.Black, new System.Drawing.PointF(270, yPos));
            yPos += (int)lineSpacing;
            g.DrawString("Payment: ", new System.Drawing.Font("Arial", 12, FontStyle.Bold), Brushes.Black, new System.Drawing.PointF(10, yPos));
            g.DrawString(Payment, new System.Drawing.Font("Arial", 12, FontStyle.Bold), Brushes.Black, new System.Drawing.PointF(270, yPos));
            yPos += (int)lineSpacing;
            g.DrawString("Change: ", new System.Drawing.Font("Arial", 12, FontStyle.Bold), Brushes.Black, new System.Drawing.PointF(10, yPos));
            g.DrawString(Change, new System.Drawing.Font("Arial", 12, FontStyle.Bold), Brushes.Black, new System.Drawing.PointF(270, yPos));
            yPos += (int)lineSpacing;

            _currentPage++;

            e.HasMorePages = _currentPage < _totalPages;
            if(e.HasMorePages == false)
            {
                _currentPage = 0;
                CartItems.Clear();
                Total = 0;
                Payment = string.Empty;
                Address = string.Empty;
                Phone = string.Empty;
            }
        }
    }
}
