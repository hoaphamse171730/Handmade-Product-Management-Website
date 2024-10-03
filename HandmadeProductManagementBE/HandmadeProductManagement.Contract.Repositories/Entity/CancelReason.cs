using HandmadeProductManagement.Core.Base;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class CancelReason : BaseEntity
    {
        public required string Description { get; set; }
        public required decimal RefundRate { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = [];
    }
}
