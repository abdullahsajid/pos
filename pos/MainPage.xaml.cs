using pos.Data;
using pos.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace pos
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private ObservableCollection<CategoryModel> _categories;
        private readonly DB_Services _dbServices;
        public bool _isBusy;
        public ObservableCollection<CategoryModel> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged(nameof(Categories));
            }
        }
        //public ICommand SelectCategoryCommand { get; }
        //public event PropertyChangedEventHandler PropertyChanged;
        //protected void OnPropertyChanged(string propertyName) =>
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));



        public MainPage()
        {
            InitializeComponent();
            _dbServices = new DB_Services();
            Task.Run(async () => await GetCategory());
            Task.Run(async () => await GetProduct());
            //    BindingContext = this;

            //    Categories = new ObservableCollection<CategoryModel>
            //{
            //    new CategoryModel { Id = 1, Name = "Bread" },
            //    new CategoryModel { Id = 2, Name = "Pastries" },
            //    new CategoryModel { Id = 3, Name = "Cakes" },
            //    new CategoryModel { Id = 4, Name = "Beverages" },
            //    new CategoryModel { Id = 5, Name = "Snacks" },
            //    new CategoryModel { Id = 6, Name = "Desserts" }
            //};

            //SelectCategoryCommand = new Command<int>(async (categoryId) =>
            //{
            //    var selectedCategory = Categories.FirstOrDefault(c => c.Id == categoryId);
            //    if (selectedCategory != null)
            //    {
            //        await DisplayAlert("Category Selected", $"Selected: {selectedCategory.Name}", "OK");
            //    }
            //});
        }

        private async Task GetCategory()
        {
            _isBusy = true;
            await _dbServices.initDatabase();
            await Task.Delay(3000);
            var categories = await _dbServices.GetCategory();
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                CategoryListView.ItemsSource = categories;
            });
            _isBusy = false;
        }

        private async Task GetProduct()
        {
            _isBusy = true;
            await _dbServices.initDatabase();
            await Task.Delay(3000);
            var products = await _dbServices.GetProducts();
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                ProductListView.ItemsSource = products;
            });
            _isBusy = false;
        }

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
