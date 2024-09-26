using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.ShopModelViews
{
    public class CreateShopDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double Rating { get; set; }
        public Guid UserId { get; set; }
    }
}
