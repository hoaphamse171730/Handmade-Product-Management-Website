using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.ProductModelViews
{
    public class ProductResponseModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CategoryId { get; set; } = string.Empty;
        public string ShopId { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Status { get; set; } = string.Empty;
        public int SoldCount { get; set; }
        public int Price { get; set; }
    }
}
