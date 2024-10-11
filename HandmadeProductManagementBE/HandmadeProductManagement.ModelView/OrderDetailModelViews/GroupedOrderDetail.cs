using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.CartModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.OrderDetailModelViews
{
    public class GroupedOrderDetail
    {
        public string ShopId { get; set; } = string.Empty;
        public CartItem CartItem { get; set; }
        public ProductItem ProductItem { get; set; }
        public decimal DiscountPrice { get; set; }
    }
}
