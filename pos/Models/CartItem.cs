﻿using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace pos.Models
{
    public partial class CartModel : ObservableObject
    {
        public int itemId { get; set; }
        public string Name { get; set; }

        public string Price { get; set; }

        [ObservableProperty,NotifyPropertyChangedFor(nameof(Total))]
        public int _quantity = 1;

        public decimal Total => _quantity * decimal.Parse(Price);

        partial void OnQuantityChanged(int value)
        {
            OnPropertyChanged(nameof(Total));
        }
    }
}
