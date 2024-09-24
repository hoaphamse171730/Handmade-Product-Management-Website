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
        [MaxLength(50)]
        public required string OrderId { get; set; }

        [MaxLength(50)]
        public required string ProductId { get; set; }
        public int ProductQuantity { get; set; }
        public float ProductPrice { get; set; }


        // Navigation properties
        //public virtual Order? Order { get; set; }
        //public virtual Product? Product { get; set; }
        //public virtual CancelReason?
    }
}
