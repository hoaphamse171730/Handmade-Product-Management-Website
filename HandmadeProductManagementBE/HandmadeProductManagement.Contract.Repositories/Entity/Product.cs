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

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CategoryId { get; set; }
        public string ShopId { get; set; }
        public int Rating { get; set; }
        public string Status { get; set; }
        public int SoldCount { get; set; }
        public ICollection<ProductImage> ProductImages { get; set; }
    }
}
