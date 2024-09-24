using HandmadeProductManagement.Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class Promotion : BaseEntity
    {
        [MaxLength(50)]
        public required string CategoryId { get; set; }
        [MaxLength(255)]
        public required string PromotionName { get; set; }
        public float DiscountRate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [MaxLength(255)]
        public string? Status { get; set; }

    }
}
