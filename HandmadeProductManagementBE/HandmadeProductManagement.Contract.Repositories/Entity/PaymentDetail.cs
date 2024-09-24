using System;
using System.ComponentModel.DataAnnotations;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Utils;


namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class PaymentDetail : BaseEntity
    {
        [Required]
        public string PaymentId { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = string.Empty;

        [Required]
        public double Amount { get; set; }

        [Required]
        public string Method { get; set; } = string.Empty;

        public string? ExternalTransaction { get; set; }

        public virtual Payment Payment { get; set; } 

        public PaymentDetail()
        {
            CreatedTime = CoreHelper.SystemTimeNow;
            LastUpdatedTime = CreatedTime;
        }
    }
}