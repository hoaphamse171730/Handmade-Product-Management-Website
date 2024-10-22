using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Repositories.Entity;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class CartItem : BaseEntity
    {
        public Guid UserId { get; set; }
        public int ProductQuantity { get; set; }
        public string ProductItemId { get; set; } = string.Empty;
        public ProductItem? ProductItem { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
