using HandmadeProductManagement.Core.Base;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class CancelReason : BaseEntity
    {
        public string? Description { get; set; }
        public float RefundRate { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = [];
    }
}
