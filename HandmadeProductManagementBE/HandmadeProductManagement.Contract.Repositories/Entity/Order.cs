using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Repositories.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class Order : BaseEntity
    {
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Note { get; set; }
        public string? CancelReasonId { get; set; }

        public ApplicationUser User { get; set; } = new ApplicationUser();
        public CancelReason CancelReason { get; set; } = new CancelReason();
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
