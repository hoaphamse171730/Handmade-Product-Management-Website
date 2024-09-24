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
        public string Id { get; set; }
        public string Url { get; set; }
        public string ProductId { get; set; }
        public Product Product { get; set; }
    }
}
