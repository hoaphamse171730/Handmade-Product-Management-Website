using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Repositories.Entity;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    internal class Cart : BaseEntity
    {
        public ApplicationUser UserId { get; set; }

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
