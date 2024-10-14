using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.OrderDetailModelViews
{
    public class OrderDetailResponseModel
    {
        public string Id { get; set; } = string.Empty;
        public int ProductQuantity { get; set; }
        public decimal Price { get; set; }
        public string ProductItemId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
    }
}
