using HandmadeProductManagement.Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class OrderDetail : BaseEntity
    {
        public string ProductItemId { get; set; }
        public string OrderId { get; set; }
        public int ProductQuantity { get; set; }
        public decimal DiscountPrice { get; set; }

        public virtual Order? Order { get; set; }
        public virtual ProductItem? ProductItem { get; set; }
      
    }
}
