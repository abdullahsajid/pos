using CommunityToolkit.Mvvm.ComponentModel;
using pos.Data;

namespace pos.Models
{
    public partial class SearchItemModel : ObservableObject
    {
        [ObservableProperty]
        private object _item;

        [ObservableProperty]
        private bool _isSelected;

        public string DisplayName => Item switch
        {
            ProductItem p => p.Name,
            Deal d => d.DealName,
            _ => "Unknown"
        };

        public decimal DisplayPrice => Item switch
        {
            ProductItem p => p.Price,
            Deal d => d.DealAmount,
            _ => 0
        };

        public SearchItemModel(object item)
        {
            Item = item;
        }
    }
}
