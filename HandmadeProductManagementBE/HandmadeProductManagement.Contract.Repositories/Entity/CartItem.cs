using HandmadeProductManagement.Core.Base;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    internal class CartItem : BaseEntity
    {
        public String CartId { get; set; }
        public Cart cart { get; set; }

        /*        public String ProductId { get; set; }
                public Product Product { get; set; }*/

        public int ProductQuantity { get; set; }
    }
}
