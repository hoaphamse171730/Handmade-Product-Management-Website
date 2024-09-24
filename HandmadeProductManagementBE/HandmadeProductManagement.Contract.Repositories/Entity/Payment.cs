using System;
using System.ComponentModel.DataAnnotations;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Utils;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class Payment : BaseEntity
    {
        public string OrderId { get; set; } = string.Empty;

        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset ExpirationDate { get; set; }

        [Required]
        public double TotalAmount { get; set; }

        public Payment()
        {
            CreatedTime = CoreHelper.SystemTimeNow;
            LastUpdatedTime = CreatedTime;
        }
    }
}