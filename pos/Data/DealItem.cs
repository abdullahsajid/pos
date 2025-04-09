using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;


namespace pos.Data
{
    public partial class DealItem : ObservableObject
    {
        [PrimaryKey,AutoIncrement]
        public long Id { get; set; }

        [ObservableProperty]
        private string _tempId = Guid.NewGuid().ToString();
        public long DealId { get; set; }

        public string DealName { get; set; }
        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        [ObservableProperty, NotifyPropertyChangedFor(nameof(SubTotal))]
        private decimal _unitPrice;

        [ObservableProperty,NotifyPropertyChangedFor(nameof(SubTotal))]
        private int _quantity = 1;

        private decimal _subTotal;

        public decimal SubTotal
        {
            get => _quantity * _unitPrice;
            set => _subTotal = value;
        }

    }
}