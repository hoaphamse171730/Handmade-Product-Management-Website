using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Repositories.Entity;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class Cart : BaseEntity
    {
        public Guid UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public ICollection<CartItem> CartItems { get; set; } = [];
    }
}
