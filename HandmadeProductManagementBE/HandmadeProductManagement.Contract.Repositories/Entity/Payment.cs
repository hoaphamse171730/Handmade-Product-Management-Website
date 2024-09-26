using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Utils;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class Payment : BaseEntity
    {
        public string OrderId { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;

        [JsonIgnore]
        public Order? Order { get; set; }

        public ICollection<PaymentDetail> PaymentDetails { get; set; } = new List<PaymentDetail>();
    }
}