using HandmadeProductManagement.Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class CancelReason : BaseEntity
    {
        public string? Description { get; set; }
        public float RefundRate { get; set; }
        // Mot Cancel Reason co the o trong nhieu Order
        public ICollection<Order> Orders { get; set; } = [];
    }
}
