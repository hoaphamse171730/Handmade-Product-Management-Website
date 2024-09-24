using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Repositories.Entity;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class Cart : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = new ApplicationUser();

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
