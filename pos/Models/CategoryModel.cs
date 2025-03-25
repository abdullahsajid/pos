using CommunityToolkit.Mvvm.ComponentModel;
using pos.Data;

namespace pos.Models
{
    public partial class CategoryModel : ObservableObject
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [ObservableProperty]
        public bool _isSelected;

        public static CategoryModel FromEntity(MenuCategory entity) => new()
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }
}
