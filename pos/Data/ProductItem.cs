using SQLite;
using CommunityToolkit.Mvvm.ComponentModel;

namespace pos.Data
{
    public partial class ProductItem : ObservableObject
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ObservableProperty]
        public string _name;

        [ObservableProperty]
        public string _description;

        [ObservableProperty]
        public decimal _price;

        [ObservableProperty]
        public int _stock;

        [ObservableProperty]
        public string _barcode;

        public int CategoryId { get; set; }

        [ObservableProperty]
        public string _imagePath;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
