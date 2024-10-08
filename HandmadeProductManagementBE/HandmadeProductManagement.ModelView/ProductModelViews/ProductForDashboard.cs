using HandmadeProductManagement.Contract.Repositories.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.ProductModelViews
{
    public class ProductForDashboard
    {
        public string Name { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty; 
        public int Price { get; set; }
        public ICollection<ProductImage> ProductImages { get; set; } = [];
    }
}
