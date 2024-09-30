using System;
using System.ComponentModel.DataAnnotations;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Utils;


namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class PaymentDetail : BaseEntity
    {
        public string PaymentId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string? ExternalTransaction { get; set; }

        public Payment? Payment { get; set; }
    }
}