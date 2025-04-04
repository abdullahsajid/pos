﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using pos.Models;
using pos.Data;
using System.Collections.ObjectModel;
using MenuItem = pos.Data.ProductItem;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Drawing;

namespace pos.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        [ObservableProperty]
        public ObservableCollection<CategoryModel> _categories = new();

        [ObservableProperty]
        public ObservableCollection<ProductModel> _products = new();

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
            //IsLoading = false;
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
        public void AddToCart(ProductModel menuItem)
        {
            try
            {
                var cartitem = CartItems.FirstOrDefault(c => c.itemId == menuItem.Id);
                if (cartitem == null)
                {
                    cartitem = new CartModel
                    {
                        itemId = menuItem.Id,
                        Name = menuItem.Name,
                        Price = menuItem.Price,
                        Quantity = 1
                    };
                    CartItems.Add(cartitem);
                    UpdateTotal();
                }
                else
                {
                    cartitem.Quantity++;
                    UpdateTotal();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error: " + e.Message);
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
        public void PrintInvoice()
        {
            Debug.WriteLine("Printing Invoice");
            PrintDocument printDoc = new PrintDocument();

            printDoc.PrintPage += PrintPageHandler;
            printDoc.Print();
            Debug.WriteLine("Invoice Printed");
        }

        public void PrintPageHandler(object sender,System.Drawing.Printing.PrintPageEventArgs e)
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

            g.DrawString("Super Sweets Bakers", new System.Drawing.Font("Arial", 20, FontStyle.Bold), Brushes.Black, new RectangleF(0, yPos, pageWidth, lineSpacing), centerFormat);
            yPos += (int)lineSpacing;

            g.DrawString("Laiq Ali Chowk Wah Cantt", new System.Drawing.Font("Arial", 17), Brushes.Black, new RectangleF(0, yPos, pageWidth, lineSpacing), centerFormat);
            yPos += (int)lineSpacing;

            g.DrawString("Phone no# 03135172181", new System.Drawing.Font("Arial", 15), Brushes.Black, new RectangleF(0, yPos, pageWidth, lineSpacing), centerFormat);
            yPos += (int)lineSpacing;

            g.DrawString($"Customer:", new System.Drawing.Font("Arial", 12,FontStyle.Bold), Brushes.Black, new System.Drawing.PointF(10, yPos));
            yPos += (int)lineSpacing;

            string dateLabel = "Date: ";
            float dateLabelWidth = g.MeasureString(dateLabel, new System.Drawing.Font("Arial", 8, FontStyle.Bold)).Width;
            g.DrawString(dateLabel, new System.Drawing.Font("Arial", 12, FontStyle.Bold), Brushes.Black, new System.Drawing.PointF(10, yPos));

            string dateValue = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            float spaceBetween = 20;
            g.DrawString(dateValue, new System.Drawing.Font("Arial", 12), Brushes.Black, new System.Drawing.PointF(10 + dateLabelWidth + spaceBetween, yPos));
            yPos += (int)lineSpacing;

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
            e.HasMorePages = false;
        }
    }
}