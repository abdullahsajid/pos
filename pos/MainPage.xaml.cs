using pos.ViewModels;

namespace pos
{
    public partial class MainPage : ContentPage
    {
        public bool _isBusy;
        //}
        //public event PropertyChangedEventHandler PropertyChanged;
        //protected void OnPropertyChanged(string propertyName) =>
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //public ICommand SelectCategoryCommand { get; }
        //public event PropertyChangedEventHandler PropertyChanged;
        //protected void OnPropertyChanged(string propertyName) =>
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public MainPage(HomeViewModel homeViewModel)
        {
            InitializeComponent();
            BindingContext = homeViewModel;

            Loaded += MainPage_Loaded;

            //SelectCategoryCommand = new Command<int>(async (categoryId) =>
            //{
            //    var selectedCategory = Categories.FirstOrDefault(c => c.Id == categoryId);
            //    if (selectedCategory != null)
            //    {
            //        await DisplayAlert("Category Selected", $"Selected: {selectedCategory.Name}", "OK");
            //    }
            //});
        }
        private async void MainPage_Loaded(object sender, EventArgs e)
        {
            await (BindingContext as HomeViewModel).InitializeAsync();
        }
 
        //private async Task GetCategory()
        //{
        //    _isBusy = true;
        //    await _dbServices.initDatabase();
        //    await Task.Delay(3000);
        //    var categories = await _dbServices.GetCategory();
        //    System.Diagnostics.Debug.WriteLine($"Categories: {categories}");
        //    await MainThread.InvokeOnMainThreadAsync(() =>
        //    {
        //        CategoryListView.ItemsSource = categories;
        //    });
        //    _isBusy = false;
        //}

        //private async Task GetProduct()
        //{
        //    _isBusy = true;
        //    await _dbServices.initDatabase();
        //    await Task.Delay(3000);
        //    var products = await _dbServices.GetProducts();
        //    Console.WriteLine($"Products: {products}");
        //    await MainThread.InvokeOnMainThreadAsync(() =>
        //    {
        //        ProductListView.ItemsSource = products;
        //    });
        //    _isBusy = false;
        //}
        //private void IncreaseQuantity(ProductModel product)
        //{
        //    if (product != null)
        //    {
        //        var cartItem = new Data.CartItem
        //        {
        //            Id = product.Id,
        //            Name = product.Name,
        //            Price = product.Price,
        //            Quantity = 1
        //        };
        //        _cartService.AddToCart(cartItem); // Add to cart
        //        product.Quantity = _cartService.GetQuantity(product.Id); // Update UI
        //    }
        //}

        //private void DecreaseQuantity(ProductModel product)
        //{
        //    if (product != null && _cartService.GetQuantity(product.Id) > 0)
        //    {
        //        _cartService.DecrementQuantity(product.Id);
        //        product.Quantity = _cartService.GetQuantity(product.Id); // Update UI
        //    }
        //}

        //protected override async void OnDisappearing()
        //{
        //    await _dbServices.DisposeAsync();
        //    base.OnDisappearing();
        //}

        //private async void OnAddProductClicked(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(NameEntry.Text) || string.IsNullOrEmpty(PriceEntry.Text))
        //        {
        //            await DisplayAlert("Error", "Please enter product name and price", "OK");
        //            return;
        //        }
        //        var newProduct = new ProductModel
        //        {
        //            Name = NameEntry.Text,
        //            Price = PriceEntry.Text
        //        };

        //        int rowsAffected = await _dbServices.AddProduct(newProduct);
        //        if (rowsAffected > 0)
        //        {
        //            await GetProduct();
        //            NameEntry.Text = string.Empty;
        //            PriceEntry.Text = string.Empty;
        //        }
        //        else
        //        {
        //            await DisplayAlert("Error", "Failed to add product.", "OK");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        //    }
        //}

        //private async void AddCategory_Clicked(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if(string.IsNullOrEmpty(NameEntry.Text))
        //        {
        //            await DisplayAlert("Error", "Please enter category name", "OK");
        //            return;
        //        }
        //        var newCategory = new CategoryModel { Name = NameEntry.Text };

        //        int result = await _dbServices.AddCategory(newCategory);
        //        if (result > 0)
        //        {
        //            await GetCategory();
        //            NameEntry.Text = string.Empty;
        //        }
        //        else
        //        {
        //            await DisplayAlert("Error", "Category not added", "OK");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await DisplayAlert("Error", ex.Message, "OK");
        //    }
        //}
    }

}
