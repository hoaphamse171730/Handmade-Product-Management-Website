using HandmadeProductManagement.Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class ProductImage : BaseEntity
    {
        public string Url { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public Product Product { get; set; } = new Product();
    }
}
