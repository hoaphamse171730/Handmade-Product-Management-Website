using HandmadeProductManagement.Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CategoryId { get; set; } = string.Empty;
        public string ShopId { get; set; } = string.Empty ;
        public int Rating { get; set; } 
        public string Status { get; set; } = string.Empty;  
        public int SoldCount { get; set; }

        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>(); 
        public ICollection<ProductItem> ProductItems { get; set; } = [];
        public Category Category { get; set; } = new Category();
        public Shop Shop { get; set; } = new Shop();
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
