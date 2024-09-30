using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.CartModelViews
{
    public class CreateCartItemDto
    {
        public string ProductItemId { get; set; } // Unique identifier for the product
        public int ProductQuantity { get; set; } // Quantity of the product in the cart
    }
}
