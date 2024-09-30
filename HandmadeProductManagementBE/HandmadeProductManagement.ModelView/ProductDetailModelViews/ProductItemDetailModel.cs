using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.ProductDetailModelViews
{
    public class ProductItemDetailModel
    {
        public string Id { get; set; }
        public int QuantityInStock { get; set; }
        public int Price { get; set; }
        public int? DiscountedPrice { get; set; }
        public List<ProductConfigurationDetailModel> Configurations { get; set; }
    }
}
