using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
namespace pos.Models
{
    public partial class ProductModel : ObservableObject
    {
        public int Id { get; set; }

        [ObservableProperty]
        public string _name;

        [ObservableProperty]
        public string _barcode;

        [ObservableProperty]
        public string _description;

        [ObservableProperty]
        public decimal _price;

        [ObservableProperty]
        public int _stock;

        //[ObservableProperty]
        //public int Categories;

        public ObservableCollection<CategoryModel> Categories { get; set; } = [];

        public CategoryModel[] SelectedCategories => Categories.Where(c => c.IsSelected).ToArray();

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public System.DateTime CreatedAt { get; set; } = System.DateTime.Now;
    }
}