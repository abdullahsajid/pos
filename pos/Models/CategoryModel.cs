using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using pos.Data;

namespace pos.Models
{
    public partial class CategoryModel : ObservableObject
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [ObservableProperty]
        public bool _isSelected;

        [RelayCommand]
        private void ToggleSelection() => IsSelected = !IsSelected; 
    }
}