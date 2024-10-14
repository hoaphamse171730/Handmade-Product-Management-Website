using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.ProductModelViews
{
    public class TopSellingProducts
    {
        public string Name { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int Price { get; set; }
        public ICollection<string> ImageUrls { get; set; } = new List<string>();

        public int SoldCount { get; set; }
    }
}
