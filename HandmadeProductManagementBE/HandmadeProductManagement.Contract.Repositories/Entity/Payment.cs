using System;
using System.ComponentModel.DataAnnotations;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Utils;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class Payment : BaseEntity
    {
        public string OrderId { get; set; } = string.Empty;

        public DateTime ExpirationDate { get; set; }

        public float TotalAmount { get; set; }

        public Order Order { get; set; } = new Order();


    }
}