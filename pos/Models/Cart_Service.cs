using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace pos.Data
//namespace pos.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Price { get; set; }
        public int Quantity { get; set; }
    }
    public class Cart_Service
    {
        //private static Cart_Service _instance;
        //private List<CartItem> _cart;
        //public event EventHandler CartChanged;
        private readonly Dictionary<int, CartItem> _cartItems;
        public Cart_Service()
        {
            _cartItems = new Dictionary<int, CartItem>();
        }

        //public static Cart_Service Instance
        //{
        //    get
        //    {
        //        if (_instance == null)
        //        {
        //            _instance = new Cart_Service();
        //        }
        //        return _instance;
        //    }
        //}

        //public List<CartItem> GetCartItems()
        //{
        //    return _cart;
        //}

        //public void AddToCart(CartItem item)
        //{
        //    var existingItem = _cart.FirstOrDefault(i => i.Id == item.Id);

        //    if (existingItem != null)
        //    {
        //        existingItem.Quantity++;
        //    }
        //    else
        //    {
        //        _cart.Add(item);
        //    }

        //    CartChanged?.Invoke(this, EventArgs.Empty);
        //}

        public void AddToCart(CartItem item)
        {
            if (_cartItems.ContainsKey(item.Id))
            {
                _cartItems[item.Id].Quantity++;
            }
            else
            {
                _cartItems[item.Id] = item;
            }
        }

        public void DecrementQuantity(int productId)
        {
            if (_cartItems.ContainsKey(productId) && _cartItems[productId].Quantity > 0)
            {
                _cartItems[productId].Quantity--;
                if (_cartItems[productId].Quantity == 0)
                {
                    _cartItems.Remove(productId);
                }
            }
        }

        public int GetQuantity(int productId)
        {
            return _cartItems.ContainsKey(productId) ? _cartItems[productId].Quantity : 0;
        }

        // Optional: Clear cart after payment
        public void ClearCart()
        {
            _cartItems.Clear();
        }

        //public decimal GetTotalAmount()
        //{
        //    return _cart.Sum(item => item.Price * item.Quantity);
        //}
    }
}