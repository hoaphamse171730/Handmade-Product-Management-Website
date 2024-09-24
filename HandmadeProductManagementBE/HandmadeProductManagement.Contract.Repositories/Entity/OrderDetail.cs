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
        
        public required string OrderDetailId { get; set; }
        public required string ProductId { get; set; }
        public required string OrderId { get; set; }
        
        public int ProductQuantity { get; set; }
        public float UnitPrice { get; set; }


        // Navigation properties
        public virtual Order? Order { get; set; }
        public virtual Product? Product { get; set; }
        
        //public Product Product { get; set; } = new Product();
    }
}
