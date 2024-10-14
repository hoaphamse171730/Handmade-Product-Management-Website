using HandmadeProductManagement.Contract.Repositories.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.CartModelViews
{
    public class CartItemModel
    {
        public string CartItemId { get; set; } // Unique identifier for the cart item, if needed
        public string ProductItemId { get; set; } // Unique identifier for the product
        public int ProductQuantity { get; set; } // Quantity of the product in the cart
        public int UnitPrice { get; set; }
        public int DiscountPrice { get; set; }
        public int TotalPriceEachProduct { get; set; }
    }
}
