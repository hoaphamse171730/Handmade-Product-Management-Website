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
        public string OrderId { get; set; } = string.Empty;

        [MaxLength(50)]
        public string ProductId { get; set; } = string.Empty ;
        public int ProductQuantity { get; set; }
        public float UnitPrice { get; set; }


        // Navigation properties
        public  Order Order { get; set; } = new Order();
        //public Product Product { get; set; } = new Product();
    }
}
