using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.CartModelViews
{
    public class CartModel
    {
        public string CartId { get; set; } // Unique identifier for the cart
        public Guid UserId { get; set; } // User identifier associated with the cart
        public List<CartItemModel> CartItems { get; set; } // List of items in the cart

        public CartModel()
        {
            CartItems = new List<CartItemModel>();
        }
    }
}