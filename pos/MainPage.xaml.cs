using pos.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace pos
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private ObservableCollection<CategoryModel> _categories;

        public ObservableCollection<CategoryModel> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged(nameof(Categories));
            }
        }
        public ICommand SelectCategoryCommand { get; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));



        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;

            Categories = new ObservableCollection<CategoryModel>
        {
            new CategoryModel { Id = 1, Name = "Bread" },
            new CategoryModel { Id = 2, Name = "Pastries" },
            new CategoryModel { Id = 3, Name = "Cakes" },
            new CategoryModel { Id = 4, Name = "Beverages" },
            new CategoryModel { Id = 5, Name = "Snacks" },
            new CategoryModel { Id = 6, Name = "Desserts" }
        };

            // Command to handle category selection
            SelectCategoryCommand = new Command<int>(async (categoryId) =>
            {
                var selectedCategory = Categories.FirstOrDefault(c => c.Id == categoryId);
                if (selectedCategory != null)
                {
                    await DisplayAlert("Category Selected", $"Selected: {selectedCategory.Name}", "OK");
                    // Later: Load products for this category
                }
            });
        }

        //private void OnCounterClicked(object sender, EventArgs e)
        //{
        //    count++;

        //    if (count == 1)
        //        CounterBtn.Text = $"Clicked {count} time";
        //    else
        //        CounterBtn.Text = $"Clicked {count} times";

        //    SemanticScreenReader.Announce(CounterBtn.Text);
        //}
    }

}
