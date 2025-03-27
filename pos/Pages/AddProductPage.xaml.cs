using pos.ViewModels;
using System.Diagnostics;
using pos.Data;
namespace pos.Pages;

public partial class AddProductPage : ContentPage
{
    private readonly DB_Services _dbServices;
    public AddProductPage(AddProductModel addProductModel, DB_Services dbServices)
	{
		InitializeComponent();
        _dbServices = dbServices;
        BindingContext = addProductModel;
        Loaded += MainPage_Loaded;
        //Task.Run(async () => await GetCategory());
    }
    private async void MainPage_Loaded(object sender, EventArgs e)
    {
        await (BindingContext as AddProductModel).InitializeAsync();
    }

    public async void saveCategory_Clicked(object sender, EventArgs e)
    {
        if(string.IsNullOrEmpty(categoryEntryField.Text))
        {
            await DisplayAlert("Error", "Please enter a category name", "OK");
            return;
        }
        var newCategory = new MenuCategory
        {
            Name = categoryEntryField.Text
        };
        Debug.WriteLine($"New Category: {newCategory.Name}");
        int result = await _dbServices.AddCategory(newCategory);
        Debug.WriteLine($"Result: {result}");
        if (result > 0)
        {
            //await GetCategory();
            categoryEntryField.Text = string.Empty;
        }
    }

    //public async void saveProduct_Clicked(object sender, EventArgs e)
    //{
    //    if (string.IsNullOrEmpty(nameEntryField.Text))
    //    {
    //        await DisplayAlert("Error", "Please enter a category name", "OK");
    //        return;
    //    }
    //    var newCategory = new ProductItem
    //    {
    //        Name = nameEntryField.Text,
    //        Price = priceEntryField.Text,
    //        CategoryId = (CategoryListView.SelectedItem as CategoryModel).Id
    //    };
    //    Debug.WriteLine($"New Category: {newCategory.Name}");
    //    int result = await _dbServices.AddCategory(newCategory);
    //    Debug.WriteLine($"Result: {result}");
    //    if (result > 0)
    //    {
    //        await GetCategory();
    //        categoryEntryField.Text = string.Empty;
    //    }
    //}

    //public async Task GetCategory()
    //{
    //    try
    //    {
    //        CategoryListView.ItemsSource = await _dbServices.GetCategory();
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Error: {ex.Message}");
    //    }
    //}

}