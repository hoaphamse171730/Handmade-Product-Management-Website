using HandmadeProductManagement.Core.Base;
using System.Text.Json.Serialization;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class CancelReason : BaseEntity
    {
        public required string Description { get; set; }
        public required decimal RefundRate { get; set; }
        [JsonIgnore]
        public virtual ICollection<Order> Orders { get; set; } = [];
    }
}
