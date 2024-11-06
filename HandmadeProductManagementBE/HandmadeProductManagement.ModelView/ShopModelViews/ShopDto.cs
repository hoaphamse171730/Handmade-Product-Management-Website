using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.ShopModelViews
{
    public class ShopDto
    {
        public string Id { get; set; } 
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Rating { get; set; }
        public decimal TotalSales { get; set; }
        public string userId { get; set; } = string.Empty;
        public string ownerName { get; set; } = string.Empty ;

    }
}
