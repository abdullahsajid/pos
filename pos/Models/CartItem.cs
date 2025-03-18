using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace pos.Models
{
    public partial class CartModel : ObservableObject
    {
        public int itemId { get; set; }
        public string Name { get; set; }

        public string Price { get; set; }

        [ObservableProperty]
        public int quantity;
    }
}
